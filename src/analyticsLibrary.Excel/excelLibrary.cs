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
using analyticsLibrary.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using ExcelDataReader;

namespace analyticsLibrary.Excel
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

        // BREAKING: getExcelConnection removed. Returning OfficeOpenXml.ExcelPackage from this library is deprecated/removed in Epic 002.
        // Migration: callers should use NPOI (XSSFWorkbook/HSSFWorkbook) or the new getWorkbookSheetDatasets APIs.
        // NOTE: The original public API exposed ExcelPackage; removing it is intentional for this epic (task 16 tracks migration).

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
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var names = new List<string>();

            if (ext == ".xlsx")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new XSSFWorkbook(fs);
                    for (int i = 0; i < workbook.NumberOfSheets; i++) names.Add(workbook.GetSheetAt(i).SheetName);
                }
            }
            else if (ext == ".xls")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new HSSFWorkbook(fs);
                    for (int i = 0; i < workbook.NumberOfSheets; i++) names.Add(workbook.GetSheetAt(i).SheetName);
                }
            }
            else if (ext == ".xlsb")
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = File.OpenRead(fileName))
                using (var reader = ExcelReaderFactory.CreateBinaryReader(stream))
                {
                    var ds = reader.AsDataSet();
                    foreach (DataTable t in ds.Tables) names.Add(t.TableName);
                }
            }
            else
            {
                throw new NotSupportedException($"Extension '{ext}' is not supported. Use .xlsx, .xls, or .xlsb.");
            }

            return compactNames ?
                names.Select(s => s.Replace("'", string.Empty).Replace("$", string.Empty)).ToArray() :
                names.ToArray();
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
                foreach (var row in skip) returnTable.Rows.Remove(row);
            }
            return returnTable;
        }
        public static DataSet getSheetData(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var returnDS = new DataSet();

            if (ext == ".xlsx")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new XSSFWorkbook(fs);
                    for (int si = 0; si < workbook.NumberOfSheets; si++)
                    {
                        var sheet = workbook.GetSheetAt(si);
                        var dsTable = new DataTable(sheet.SheetName);

                        if (sheet.LastRowNum < sheet.FirstRowNum)
                        {
                            returnDS.Tables.Add(dsTable);
                            continue;
                        }

                        int firstRow = sheet.FirstRowNum;
                        int lastRow = sheet.LastRowNum;
                        IRow headerRow = sheet.GetRow(firstRow);
                        int firstCol = 0;
                        int lastCol = 0;
                        if (headerRow != null)
                        {
                            firstCol = headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0;
                            lastCol = headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol;
                        }
                        else
                        {
                            for (int r = firstRow; r <= lastRow; r++)
                            {
                                var row = sheet.GetRow(r);
                                if (row != null)
                                {
                                    firstCol = row.FirstCellNum >= 0 ? row.FirstCellNum : 0;
                                    lastCol = row.LastCellNum > 0 ? row.LastCellNum - 1 : firstCol;
                                    break;
                                }
                            }
                        }

                        var table = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                        returnDS.Tables.Add(table);
                    }
                }
            }
            else if (ext == ".xls")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new HSSFWorkbook(fs);
                    for (int si = 0; si < workbook.NumberOfSheets; si++)
                    {
                        var sheet = workbook.GetSheetAt(si);
                        if (sheet.LastRowNum < sheet.FirstRowNum)
                        {
                            returnDS.Tables.Add(new DataTable(sheet.SheetName));
                            continue;
                        }
                        int firstRow = sheet.FirstRowNum;
                        int lastRow = sheet.LastRowNum;
                        IRow headerRow = sheet.GetRow(firstRow);
                        int firstCol = headerRow != null && headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0;
                        int lastCol = headerRow != null && headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol;
                        var table = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                        returnDS.Tables.Add(table);
                    }
                }
            }
            else if (ext == ".xlsb")
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = File.OpenRead(fileName))
                using (var reader = ExcelReaderFactory.CreateBinaryReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };
                    var dsFromReader = reader.AsDataSet(conf);
                    foreach (DataTable table in dsFromReader.Tables)
                    {
                        var dt = table.Copy();
                        if (string.IsNullOrEmpty(dt.TableName)) dt.TableName = "Sheet";
                        returnDS.Tables.Add(dt);
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Extension '{ext}' is not supported. Use .xlsx, .xls, or .xlsb.");
            }

            return returnDS;
        }

        /// <summary>
        /// Reads a workbook and returns an ordered array of DataSet instances - one per worksheet.
        /// For .xlsx sheets each Excel structured table (ListObject) becomes a DataTable; when no tables exist a single DataTable is produced from the used range.
        /// Supported extensions: .xlsx, .xls, .xlsb
        /// DataSet.DataSetName is set to the worksheet name.
        /// </summary>
        public static DataSet[] getWorkbookSheetDatasets(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            var datasets = new List<DataSet>();

            if (ext == ".xlsx")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new XSSFWorkbook(fs);
                    for (int si = 0; si < workbook.NumberOfSheets; si++)
                    {
                        var sheet = workbook.GetSheetAt(si);
                        var ds = new DataSet();
                        ds.DataSetName = sheet.SheetName;

                        var xssfSheet = sheet as XSSFSheet;
                        var tables = xssfSheet?.GetTables();

                        if (tables != null && tables.Count > 0)
                        {
                            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            foreach (var table in tables)
                            {
                                dynamic t = table;
                                int startRow = (int)t.StartRowIndex;
                                int endRow = (int)t.EndRowIndex;
                                int startCol = (int)t.StartColIndex;
                                int endCol = (int)t.EndColIndex;
                                string tname = t.Name != null ? t.Name.ToString() : "Table";
                                if (nameCounts.ContainsKey(tname))
                                {
                                    nameCounts[tname]++;
                                    tname = $"{tname}_{nameCounts[tname]}";
                                }
                                else
                                {
                                    nameCounts[tname] = 0;
                                }

                                var dt = BuildDataTableFromNpoiSheet(sheet, startRow, endRow, startCol, endCol, tname);
                                ds.Tables.Add(dt);
                            }
                        }
                        else
                        {
                            // whole-sheet fallback
                            if (sheet.LastRowNum < sheet.FirstRowNum)
                            {
                                ds.Tables.Add(new DataTable(sheet.SheetName));
                            }
                            else
                            {
                                int firstRow = sheet.FirstRowNum;
                                int lastRow = sheet.LastRowNum;
                                IRow headerRow = sheet.GetRow(firstRow);
                                int firstCol = 0;
                                int lastCol = 0;
                                if (headerRow != null)
                                {
                                    firstCol = headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0;
                                    lastCol = headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol;
                                }
                                else
                                {
                                    // find first non-empty row
                                    for (int r = firstRow; r <= lastRow; r++)
                                    {
                                        var row = sheet.GetRow(r);
                                        if (row != null)
                                        {
                                            firstCol = row.FirstCellNum >= 0 ? row.FirstCellNum : 0;
                                            lastCol = row.LastCellNum > 0 ? row.LastCellNum - 1 : firstCol;
                                            break;
                                        }
                                    }
                                }
                                var dt = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                                ds.Tables.Add(dt);
                            }
                        }

                        datasets.Add(ds);
                    }
                }
            }
            else if (ext == ".xls")
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var workbook = new HSSFWorkbook(fs);
                    for (int si = 0; si < workbook.NumberOfSheets; si++)
                    {
                        var sheet = workbook.GetSheetAt(si);
                        var ds = new DataSet { DataSetName = sheet.SheetName };

                        if (sheet.LastRowNum < sheet.FirstRowNum)
                        {
                            ds.Tables.Add(new DataTable(sheet.SheetName));
                        }
                        else
                        {
                            int firstRow = sheet.FirstRowNum;
                            int lastRow = sheet.LastRowNum;
                            IRow headerRow = sheet.GetRow(firstRow);
                            int firstCol = headerRow != null && headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0;
                            int lastCol = headerRow != null && headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol;
                            var dt = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                            ds.Tables.Add(dt);
                        }

                        datasets.Add(ds);
                    }
                }
            }
            else if (ext == ".xlsb")
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = File.OpenRead(fileName))
                {
                    using (var reader = ExcelReaderFactory.CreateBinaryReader(stream))
                    {
                        var conf = new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                        };
                        var dsFromReader = reader.AsDataSet(conf);
                        foreach (DataTable table in dsFromReader.Tables)
                        {
                            var ds = new DataSet { DataSetName = table.TableName };
                            ds.Tables.Add(table.Copy());
                            datasets.Add(ds);
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Extension '{ext}' is not supported. Use .xlsx, .xls, or .xlsb.");
            }

            return datasets.ToArray();
        }

        /// <summary>
        /// Stream overload: provide a stream and a format hint (".xlsx", ".xls", ".xlsb").
        /// </summary>
        public static DataSet[] getWorkbookSheetDatasets(Stream stream, string formatHintOrExtension)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(formatHintOrExtension)) throw new ArgumentNullException(nameof(formatHintOrExtension));
            var ext = formatHintOrExtension.Trim().ToLowerInvariant();
            if (!ext.StartsWith('.')) ext = "." + ext;

            var datasets = new List<DataSet>();

            if (ext == ".xlsx")
            {
                var workbook = new XSSFWorkbook(stream);
                for (int si = 0; si < workbook.NumberOfSheets; si++)
                {
                    var sheet = workbook.GetSheetAt(si);
                    var ds = new DataSet { DataSetName = sheet.SheetName };
                    var xssfSheet = sheet as XSSFSheet;
                    var tables = xssfSheet?.GetTables();

                    if (tables != null && tables.Count > 0)
                    {
                        var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (var table in tables)
                        {
                            dynamic t = table;
                            int startRow = (int)t.StartRowIndex;
                            int endRow = (int)t.EndRowIndex;
                            int startCol = (int)t.StartColIndex;
                            int endCol = (int)t.EndColIndex;
                            string tname = t.Name != null ? t.Name.ToString() : "Table";
                            if (nameCounts.ContainsKey(tname))
                            {
                                nameCounts[tname]++;
                                tname = $"{tname}_{nameCounts[tname]}";
                            }
                            else nameCounts[tname] = 0;

                            var dt = BuildDataTableFromNpoiSheet(sheet, startRow, endRow, startCol, endCol, tname);
                            ds.Tables.Add(dt);
                        }
                    }
                    else
                    {
                        if (sheet.LastRowNum < sheet.FirstRowNum)
                        {
                            ds.Tables.Add(new DataTable(sheet.SheetName));
                        }
                        else
                        {
                            int firstRow = sheet.FirstRowNum;
                            int lastRow = sheet.LastRowNum;
                            IRow headerRow = sheet.GetRow(firstRow);
                            int firstCol = headerRow != null ? (headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0) : 0;
                            int lastCol = headerRow != null ? (headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol) : firstCol;
                            var dt = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                            ds.Tables.Add(dt);
                        }
                    }
                    datasets.Add(ds);
                }
            }
            else if (ext == ".xls")
            {
                var workbook = new HSSFWorkbook(stream);
                for (int si = 0; si < workbook.NumberOfSheets; si++)
                {
                    var sheet = workbook.GetSheetAt(si);
                    var ds = new DataSet { DataSetName = sheet.SheetName };
                    if (sheet.LastRowNum < sheet.FirstRowNum)
                    {
                        ds.Tables.Add(new DataTable(sheet.SheetName));
                    }
                    else
                    {
                        int firstRow = sheet.FirstRowNum;
                        int lastRow = sheet.LastRowNum;
                        IRow headerRow = sheet.GetRow(firstRow);
                        int firstCol = headerRow != null && headerRow.FirstCellNum >= 0 ? headerRow.FirstCellNum : 0;
                        int lastCol = headerRow != null && headerRow.LastCellNum > 0 ? headerRow.LastCellNum - 1 : firstCol;
                        var dt = BuildDataTableFromNpoiSheet(sheet, firstRow, lastRow, firstCol, lastCol, sheet.SheetName);
                        ds.Tables.Add(dt);
                    }
                    datasets.Add(ds);
                }
            }
            else if (ext == ".xlsb")
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateBinaryReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };
                    var dsFromReader = reader.AsDataSet(conf);
                    foreach (DataTable table in dsFromReader.Tables)
                    {
                        var ds = new DataSet { DataSetName = table.TableName };
                        ds.Tables.Add(table.Copy());
                        datasets.Add(ds);
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Extension '{ext}' is not supported. Use .xlsx, .xls, or .xlsb.");
            }

            return datasets.ToArray();
        }

        private static DataTable BuildDataTableFromNpoiSheet(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol, string tableName)
        {
            var table = new DataTable(tableName);

            // header
            IRow headerRow = sheet.GetRow(firstRow);
            for (int c = firstCol; c <= lastCol; c++)
            {
                string colName = null;
                if (headerRow != null)
                {
                    var cell = headerRow.GetCell(c);
                    if (cell != null)
                        colName = cell.ToString();
                }
                if (string.IsNullOrWhiteSpace(colName))
                    colName = $"Column {c - firstCol + 1}";

                string uniqueName = colName;
                int suffix = 1;
                while (table.Columns.Cast<DataColumn>().Any(dc => dc.ColumnName == uniqueName))
                {
                    uniqueName = $"{colName} {suffix}";
                    suffix++;
                }
                table.Columns.Add(uniqueName);
            }

            // data rows
            for (int r = firstRow + 1; r <= lastRow; r++)
            {
                var newRow = table.NewRow();
                var sheetRow = sheet.GetRow(r);
                for (int c = firstCol; c <= lastCol; c++)
                {
                    if (sheetRow != null)
                    {
                        var cell = sheetRow.GetCell(c);
                        newRow[c - firstCol] = cell != null ? cell.ToString() : string.Empty;
                    }
                    else
                    {
                        newRow[c - firstCol] = string.Empty;
                    }
                }
                table.Rows.Add(newRow);
            }

            table.TableName = tableName;
            return table;
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
