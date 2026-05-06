using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using analyticsLibrary.dbObjects;
using analyticsLibrary.library;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace analyticsLibrary.excelLibrary
{
    public static class excelLibrary
    {
        private static string connectionString(string fileName)
        {
            return string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Excel 12.0 Xml;HDR=YES;Data Source={0};", fileName);
        }
        private const string TABLE_NAME_PROPERTY = "TableName";

        private struct ExcelDataTypes
        {
            public const string NUMBER = "NUMBER";
            public const string DATETIME = "DATETIME";
            public const string STRING = "STRING";
        }
        private struct ClassDataTypes
        {
            public const string SHORT = "int16";
            public const string INT = "int32";
            public const string LONG = "int64";
            public const string STRING = "string";
            public const string DATE = "datetime";
            public const string BOOL = "boolean";
            public const string DECIMAL = "decimal";
        }

        public static ExcelPackage getExcelConnection(string fileName) {
            var package = new ExcelPackage();
            package.Load(File.OpenRead(fileName));
            return package;
        }

        public static string getExcelSheetName(DataTable dataTable)
        {
            string retVal = dataTable.TableName;
            if (dataTable.ExtendedProperties.ContainsKey(TABLE_NAME_PROPERTY))
            {
                retVal = dataTable.ExtendedProperties[TABLE_NAME_PROPERTY].ToString();
            }
            return retVal.Replace(' ', '_');
        }
        public static string[] getExcelSheetNames(string fileName)
        {
            return getExcelSheetNames(fileName, false);
        }
        public static string[] getExcelSheetNames(string fileName, bool compactNames)
        {
            string[] excelSheets = null;

            using (var package = getExcelConnection(fileName))
            {
                excelSheets = package.Workbook.Worksheets.Cast<ExcelWorksheet>()
                    .Select(s => s.Name).ToArray();

                package.Dispose();
            }

            return compactNames ?
                excelSheets
                    .Select(s => 
                        s.Replace("'", string.Empty)
                        .Replace("$", string.Empty))
                    .ToArray() :
                excelSheets;
        }
        
        public static DataTable getSheetData(string fileName, int sheetNumber, int? skipRows = null)
        {
            var data = getSheetData(fileName);
            if (data.Tables.Count < sheetNumber)
            {
                throw new ApplicationException("Sheet number does not exist for this dataset.");
            }
            return data.Tables[sheetNumber];
        }
        public static DataTable getSheetData(string fileName, string sheetName, int? skipRows = null)
        {
            var data = getSheetData(fileName);
            if (!data.Tables.Contains(sheetName))
            {
                throw new ApplicationException("Sheet name does not exist for this dataset.");
            }
            var returnTable = data.Tables[sheetName];
            if (skipRows != null)
            {
                var skip = returnTable.Rows.Cast<DataRow>()
                        .Take((int)skipRows)
                        .ToArray();
                var header = skip.Last();
                for (var i = 0; i < returnTable.Columns.Count; i++)
                    returnTable.Columns[i].ColumnName = header[i].ToString();
                skip.forEach(row => returnTable.Rows.Remove(row));
            }
            return returnTable;
        }
        public static DataSet getSheetData(string fileName)
        {
            var returnDS = new DataSet();
            var package = getExcelConnection(fileName);

            foreach (var sheet in package.Workbook.Worksheets)
            {
                var table = new DataTable(sheet.Name);
                if (sheet.Dimension != null)
                {
                    var endColumn = sheet.Dimension.End.Column;
                    var endRow = sheet.Dimension.End.Row;
                    var header = sheet.Cells[1, 1, 1, endColumn];
                    for (var i = 1; i <= endColumn; i++)
                    {
                        var cell = sheet.Cells[1, i, 1, i];
                        var columnName = !string.IsNullOrWhiteSpace(cell.Text) ? cell.Text : string.Format("Column {0}", i);
                        var usedCount = table.Columns.Cast<DataColumn>().Count(c => c.ColumnName == columnName);
                        while (usedCount > 0)
                        {
                            columnName = string.Format("{0} {1}", columnName, usedCount);
                            usedCount = table.Columns.Cast<DataColumn>().Count(c => c.ColumnName == columnName);
                        }
                        table.Columns.Add(columnName);
                    }

                    for (var i = 2; i <= endRow; i++)
                    {
                        var row = table.NewRow();
                        var values = sheet.Cells[i, 1, i, endColumn ];
                        foreach (var value in values)
                        {
                            row[value.Start.Column - 1] = value.Value;
                        }
                        table.Rows.Add(row);
                    }
                }
                returnDS.Tables.Add(table);
            }
            
            package.Dispose();
            return returnDS;
        }

        public static ExcelWorksheet getSheet(string fileName, string sheetName)
        {
            ExcelPackage package;
            return getSheet(getWorkbook(fileName, out package), sheetName);
        }
        public static ExcelWorksheet getSheet(ExcelWorkbook workbook, string sheetName)
        {
            var sheet = null as ExcelWorksheet;
            try
            {
                if (!string.IsNullOrEmpty(sheetName))
                    sheet = workbook.Worksheets[sheetName] as ExcelWorksheet;
            }
            catch { }
            if (sheet == null)
            {
                var newSheetName = "Sheet1";

                if (string.IsNullOrEmpty(sheetName))
                {
                    var sheetNumber = new Func<string, int?>(name =>
                    {
                        var returnNumber = null as int?;
                        var nameFormat = @"^Sheet(?<number>([0-9]+))$";
                        var matches = Regex.Match(name, nameFormat);
                        if (matches.Success)
                            returnNumber = int.Parse(matches.Groups["number"].ToString());

                        return returnNumber;
                    });

                    var sheetNames = workbook.Worksheets.Cast<ExcelWorksheet>()
                        .Select(s => s.Name)
                        .Where(s => sheetNumber(s) != null)
                        .Select(s => (int)sheetNumber(s));

                    newSheetName = string.Format("Sheet{0}",
                        sheetNames.Count() > 0 ? (sheetName.Max() + 1).ToString() : "1");
                }

                sheet = workbook.Worksheets.Add(string.IsNullOrEmpty(sheetName) ? newSheetName : sheetName);
            }
            return sheet;
        }
        
        public static void copySheet(string fileName, string originalSheetName, string newSheetName)
        {
            ExcelPackage package;
            var workbook = getWorkbook(fileName, out package);
            workbook.Worksheets.Copy(originalSheetName, newSheetName);
            package.Save();
        }

        //write sheet data
        public static void deleteTable(string filename, string sheetName, string tableName) {
            writeSheetDataXlsx(filename, sheetName, sheet => {
                if(sheet.Tables[tableName] != null) sheet.Tables.Delete(tableName);
            });
        }
        
        public static void writeSheetDataXlsx<t>(string fileName, string sheetName, IEnumerable<t> data)
        {
            writeSheetDataXlsx(fileName, sheetName, data,
                sheet =>
                {
                    sheet.autoSizeColumns();
                });
        }
        public static void writeSheetDataXlsx<t>(string fileName, string sheetName, IEnumerable<t> data, Action<ExcelWorksheet> formattingMethod)
        {
            writeSheetDataXlsx(fileName, sheetName,
                new Action<ExcelWorksheet>(sheet =>
                {
                    //build header
                    var type = typeof(t);
                    var fields = dataLibrary.getColumnProperties(type);
                    var row = 1;
                    var column = 1;
                    foreach (var field in fields)
                    {
                        sheet.Cells[row, column++].Value = field.Name;
                    }
                    var headerRange = sheet.Cells[row, 1, row, column];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    row++;
                    foreach (var item in data)
                    {
                        column = 1;
                        var values = fields
                            .Select(v => new { field = v, value = v.GetValue(item, null) })
                            .ToDictionary(v => v.field, v => v.value);

                        foreach (var field in fields)
                            sheet.Cells[row, column++].Value = values[field];

                        row++;
                    }
                }), formattingMethod, false);


        }

        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data)
        {
            writeSheetDataXlsx(fileName, sheetName, data, null);
        }
        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data, Action<ExcelWorksheet> formattingMethod)
        {
            writeSheetDataXlsx(fileName, sheetName, data, formattingMethod, false);
        }

        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data, bool deleteExisting)
        {
            writeSheetDataXlsx(fileName, sheetName, data, null, deleteExisting);
        }
        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data, Action<ExcelWorksheet> formattingMethod, bool deleteExisting)
        {
            writeSheetDataXlsx(fileName, sheetName, new Action<ExcelWorksheet>(sheet =>
            {
                var rows = data.GetLength(0);
                var columns = data.GetLength(1);

                for (var i = 0; i < rows; i++)
                {
                    for (var j = 0; j < columns; j++)
                    {
                        sheet.Cells[i + 1, j + 1].Value = data[i, j];
                    }
                }
                if (formattingMethod != null)
                {
                    formattingMethod(sheet);
                }
            }), deleteExisting);
        }

        public static void writeSheetDataXlsx(string fileName, string sheetName, Action<ExcelWorksheet> sheetMethod)
        {
            writeSheetDataXlsx(fileName, sheetName, sheetMethod, false);
        }
        public static void writeSheetDataXlsx(string fileName, string sheetName, Action<ExcelWorksheet> sheetMethod, bool deleteExisting)
        {
            writeSheetDataXlsx(fileName, sheetName, sheetMethod, null, deleteExisting);
        }
        public static void writeSheetDataXlsx(string fileName, string sheetName, Action<ExcelWorksheet> sheetMethod, Action<ExcelWorksheet> formattingMethod, bool deleteExisting)
        {
            if (deleteExisting && File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            ExcelPackage package;
            var workBook = getWorkbook(fileName, out package);
            var sheet = getSheet(workBook, sheetName);
            sheetMethod(sheet);
            if (formattingMethod != null)
                formattingMethod(sheet); ;
            package.Save();
            workBook.Dispose();
            sheet.Dispose();
            package.Dispose();
        }
        private static ExcelWorkbook getWorkbook(string fileName, out ExcelPackage package)
        {
            package = new ExcelPackage(new FileInfo(fileName));
            return package.Workbook;
        }
        
        private static Dictionary<string, string> getExcelDataTypeList()
        {
            Dictionary<string, string> dataTypeList = new Dictionary<string, string>();

            dataTypeList.Add(ClassDataTypes.SHORT, ExcelDataTypes.NUMBER);
            dataTypeList.Add(ClassDataTypes.INT, ExcelDataTypes.NUMBER);
            dataTypeList.Add(ClassDataTypes.LONG, ExcelDataTypes.NUMBER);
            dataTypeList.Add(ClassDataTypes.STRING, ExcelDataTypes.STRING);
            dataTypeList.Add(ClassDataTypes.DATE, ExcelDataTypes.DATETIME);
            dataTypeList.Add(ClassDataTypes.BOOL, ExcelDataTypes.STRING);
            dataTypeList.Add(ClassDataTypes.DECIMAL, ExcelDataTypes.NUMBER);

            return dataTypeList;
        }
        private static string getCreateTableCommand(DataTable dataTable)
        {
            Dictionary<string, string> dataTypeList = getExcelDataTypeList();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CREATE TABLE [{0}] (", getExcelSheetName(dataTable));
            foreach (DataColumn col in dataTable.Columns)
            {
                string type = ExcelDataTypes.STRING;
                if (dataTypeList.ContainsKey(col.DataType.Name.ToString().ToLower()))
                {
                    type = dataTypeList[col.DataType.Name.ToString().ToLower()];
                }
                sb.AppendFormat("[{0}] {1},", col.Caption.Replace(' ', '_'), type);
            }
            sb = sb.Replace(',', ')', sb.ToString().LastIndexOf(','), 1);

            return sb.ToString();
        }
        private static string getInsertCommand(DataTable dataTable, int rowIndex)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("INSERT INTO [{0}$](", getExcelSheetName(dataTable));
            foreach (DataColumn col in dataTable.Columns)
            {
                sb.AppendFormat("[{0}],", col.Caption.Replace(' ', '_'));
            }
            sb = sb.Replace(',', ')', sb.ToString().LastIndexOf(','), 1);
            sb.Append("VALUES (");
            foreach (DataColumn col in dataTable.Columns)
            {
                sb.AppendFormat("'{0}',", dataTable.Rows[rowIndex][col].ToString());
            }
            sb = sb.Replace(',', ')', sb.ToString().LastIndexOf(','), 1);
            return sb.ToString();
        }

        public static void writeWorkbook(string fileName, DataTable dataTable, Action<ExcelWorksheet> formattingMethod)
        {
            writeWorkbook(fileName, dataTable, false, formattingMethod);
        }
        public static void writeWorkbook(string fileName, DataTable dataTable)
        {
            writeWorkbook(fileName, dataTable, false);
        }
        public static void writeWorkbook(string fileName, DataTable dataTable, bool deleteExistFile)
        {
            writeWorkbook(fileName, dataTable, deleteExistFile, null);
        }
        public static void writeWorkbook(string fileName, DataTable dataTable, bool deleteExistFile, Action<ExcelWorksheet> formattingMethod)
        {
            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            writeWorkbook(fileName, dataSet, deleteExistFile);

            if (formattingMethod != null)
            {
                var package = new ExcelPackage(new FileInfo(fileName));
                var workbook = package.Workbook;
                var sheet = getSheet(workbook, dataTable.TableName);
                formattingMethod(sheet);
            }
        }

        public static void writeWorkbook(string fileName, DataSet dataSet, bool deleteExistFile)
        {
            if (deleteExistFile && File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            writeWorkbook(fileName, dataSet);
        }
        public static void writeWorkbook(string fileName, DataSet dataSet)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString(fileName)))
                {
                    connection.Open();

                    foreach (DataTable dt in dataSet.Tables)
                    {
                        var command = new OleDbCommand(getCreateTableCommand(dt), connection);
                        command.ExecuteNonQuery();

                        for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                        {
                            command = new OleDbCommand(getInsertCommand(dt, rowIndex), connection);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static void removeWorksheet(string fileName, string sheet)
        {
            removeWorksheet(fileName, (object)sheet);
        }
        public static void removeWorksheet(string fileName, int sheet)
        {
            removeWorksheet(fileName, (object)sheet);
        }
        private static void removeWorksheet(string fileName, object sheet)
        {
            var package = (ExcelPackage)null;
            var workbook = getWorkbook(fileName, out package);
            if (sheet is int)
                workbook.Worksheets.Delete((int)sheet);
            else if (sheet is string)
                workbook.Worksheets.Delete((string)sheet);
            else
                throw new ApplicationException("Sheet id type must be an int or a string value.");

            package.Save();
        }

        public static void numberFormatRow(ExcelWorksheet sheet, int row, string format)
        {
            var formatRow = sheet.Row(row);
            formatRow.Style.Numberformat.Format = format;
        }
        public static void numberFormatColumn(ExcelWorksheet sheet, int column, string format)
        {
            var formatColumn = sheet.Column(column);
            formatColumn.Style.Numberformat.Format = format;
        }
    }
}
