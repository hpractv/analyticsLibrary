using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using analyticsLibrary.Core;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using ExcelDataReader;

namespace analyticsLibrary.Excel
{
    public static class excelLibrary
    {
        private const string TABLE_NAME_PROPERTY = "TableName";

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
                using (var reader = ExcelReaderFactory.CreateReader(stream))
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
                using (var reader = ExcelReaderFactory.CreateReader(stream))
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
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
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
                using (var reader = ExcelReaderFactory.CreateReader(stream))
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

        // -------------------------------------------------------------------------
        // Write APIs — implemented with NPOI (Apache-2.0). No EPPlus or OleDb.
        // BREAKING from pre-3.0: EPPlus-based formatting overloads removed;
        // use the object[,] or IEnumerable<T> overloads instead.
        // Writing .xlsb is not supported; pass .xlsx paths to write APIs.
        // -------------------------------------------------------------------------

        private static XSSFWorkbook OpenOrCreateXssf(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (var fs = File.OpenRead(fileName))
                    return new XSSFWorkbook(fs);
            }
            return new XSSFWorkbook();
        }

        private static void SaveXssf(XSSFWorkbook workbook, string fileName)
        {
            using (var fs = File.Create(fileName))
                workbook.Write(fs);
        }

        private static ISheet GetOrCreateSheet(XSSFWorkbook workbook, string sheetName)
        {
            return workbook.GetSheet(sheetName) ?? workbook.CreateSheet(sheetName);
        }

        private static void SetCellValue(ICell cell, object value)
        {
            if (value == null) { cell.SetCellValue(string.Empty); return; }
            if (value is double d) { cell.SetCellValue(d); return; }
            if (value is float f) { cell.SetCellValue(f); return; }
            if (value is int i) { cell.SetCellValue(i); return; }
            if (value is long l) { cell.SetCellValue(l); return; }
            if (value is short s) { cell.SetCellValue(s); return; }
            if (value is decimal dec) { cell.SetCellValue((double)dec); return; }
            if (value is bool b) { cell.SetCellValue(b); return; }
            if (value is DateTime dt) { cell.SetCellValue(dt.ToString("o")); return; }
            cell.SetCellValue(value.ToString());
        }

        private static void AssertXlsxExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (ext == ".xlsb")
                throw new NotSupportedException("Writing .xlsb is not supported. Use .xlsx instead.");
        }

        /// <summary>
        /// Writes an object grid to the named sheet of an .xlsx file.
        /// Row 0 of <paramref name="data"/> is treated as the first row (no automatic header).
        /// Creates the file or sheet if absent; does not delete existing sheets.
        /// </summary>
        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data)
        {
            writeSheetDataXlsx(fileName, sheetName, data, false);
        }

        /// <summary>
        /// Writes an object grid to the named sheet of an .xlsx file.
        /// When <paramref name="deleteExisting"/> is true the file is deleted before writing.
        /// </summary>
        public static void writeSheetDataXlsx(string fileName, string sheetName, object[,] data, bool deleteExisting)
        {
            AssertXlsxExtension(fileName);
            if (deleteExisting && File.Exists(fileName)) File.Delete(fileName);

            var workbook = OpenOrCreateXssf(fileName);
            var sheet = GetOrCreateSheet(workbook, sheetName);

            var rows = data.GetLength(0);
            var cols = data.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                var row = sheet.GetRow(r) ?? sheet.CreateRow(r);
                for (int c = 0; c < cols; c++)
                    SetCellValue(row.GetCell(c) ?? row.CreateCell(c), data[r, c]);
            }

            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Writes a strongly-typed enumerable to the named sheet of an .xlsx file.
        /// The first row will contain property names as column headers.
        /// </summary>
        public static void writeSheetDataXlsx<T>(string fileName, string sheetName, IEnumerable<T> data)
        {
            AssertXlsxExtension(fileName);

            var workbook = OpenOrCreateXssf(fileName);
            var sheet = GetOrCreateSheet(workbook, sheetName);

            var fields = dataLibrary.getColumnProperties(typeof(T));
            int rowIdx = 0;

            // header
            var headerRow = sheet.GetRow(rowIdx) ?? sheet.CreateRow(rowIdx++);
            for (int c = 0; c < fields.Length; c++)
                SetCellValue(headerRow.GetCell(c) ?? headerRow.CreateCell(c), fields[c].Name);

            // data rows
            foreach (var item in data)
            {
                var row = sheet.GetRow(rowIdx) ?? sheet.CreateRow(rowIdx++);
                for (int c = 0; c < fields.Length; c++)
                    SetCellValue(row.GetCell(c) ?? row.CreateCell(c), fields[c].GetValue(item, null));
            }

            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Removes the named Excel structured table (ListObject) from a sheet in an .xlsx file.
        /// The cell data is preserved; only the table definition is removed.
        /// </summary>
        public static void deleteTable(string fileName, string sheetName, string tableName)
        {
            AssertXlsxExtension(fileName);
            var workbook = OpenOrCreateXssf(fileName);
            var sheet = workbook.GetSheet(sheetName) as XSSFSheet;
            if (sheet == null) { SaveXssf(workbook, fileName); return; }

            // NPOI does not expose a RemoveTable API. Full removal requires three steps so that
            // the table is not re-loaded when the saved file is re-opened:
            //   1. Remove from the private 'tables' field so GetTables() on the current instance returns nothing.
            //   2. Remove the <tablePart r:id="..."/> from CT_Worksheet so the sheet XML does not reference
            //      the table part (prevents re-loading on next open).
            //   3. Unregister the table package part via the protected RemoveRelation method (reflection) so
            //      NPOI does not re-write the table XML to the ZIP archive on save.
            var tablesField = typeof(XSSFSheet).GetField("tables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tablesField?.GetValue(sheet) is Dictionary<string, XSSFTable> tablesDict)
            {
                var key = tablesDict.Keys.FirstOrDefault(k =>
                    string.Equals(tablesDict[k].Name, tableName, StringComparison.OrdinalIgnoreCase));
                if (key != null)
                {
                    var table = tablesDict[key];
                    tablesDict.Remove(key);

                    // Step 2: remove <tablePart r:id="..."/> from CT_Worksheet.
                    // GetRelationId returns the r:id for a given part; the same id is used in
                    // CT_TablePart.id. The dict key itself is also typically the r:id in NPOI.
                    var ctSheet = sheet.GetCTWorksheet();
                    if (ctSheet.tableParts?.tablePart != null)
                    {
                        // Try to get the exact r:id; fall back to removing by table name match.
                        string relId = null;
                        try { relId = sheet.GetRelationId(table); } catch { }
                        if (relId != null)
                            ctSheet.tableParts.tablePart.RemoveAll(tp => tp.id == relId);
                        else
                            // The dict key in NPOI is typically the r:id.
                            ctSheet.tableParts.tablePart.RemoveAll(tp => tp.id == key);
                    }

                    // Step 3: call the protected RemoveRelation via reflection so the table part
                    // is deregistered and not written to the ZIP.
                    var removeRel = typeof(XSSFSheet).BaseType?.GetMethod("RemoveRelation",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null, new[] { typeof(XSSFTable).BaseType }, null);
                    removeRel?.Invoke(sheet, new object[] { table });
                }
            }
            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Copies a worksheet within an .xlsx file.
        /// </summary>
        public static void copySheet(string fileName, string originalSheetName, string newSheetName)
        {
            AssertXlsxExtension(fileName);
            var workbook = OpenOrCreateXssf(fileName);
            int srcIdx = workbook.GetSheetIndex(originalSheetName);
            if (srcIdx < 0) throw new ArgumentException($"Sheet '{originalSheetName}' not found in '{fileName}'.");
            int newIdx = workbook.NumberOfSheets;
            workbook.CloneSheet(srcIdx);
            workbook.SetSheetName(newIdx, newSheetName);
            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Removes a worksheet by name from an .xlsx file.
        /// </summary>
        public static void removeWorksheet(string fileName, string sheet)
        {
            AssertXlsxExtension(fileName);
            var workbook = OpenOrCreateXssf(fileName);
            int idx = workbook.GetSheetIndex(sheet);
            if (idx >= 0) workbook.RemoveSheetAt(idx);
            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Removes a worksheet by index (0-based) from an .xlsx file.
        /// </summary>
        public static void removeWorksheet(string fileName, int sheetIndex)
        {
            AssertXlsxExtension(fileName);
            var workbook = OpenOrCreateXssf(fileName);
            if (sheetIndex >= 0 && sheetIndex < workbook.NumberOfSheets)
                workbook.RemoveSheetAt(sheetIndex);
            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Writes a DataSet to an .xlsx file — one sheet per DataTable.
        /// Supported extension: .xlsx only. Writing .xlsb is not supported.
        /// </summary>
        public static void writeWorkbook(string fileName, DataSet dataSet)
        {
            writeWorkbook(fileName, dataSet, false);
        }

        /// <summary>
        /// Writes a DataSet to an .xlsx file, optionally deleting any existing file first.
        /// </summary>
        public static void writeWorkbook(string fileName, DataSet dataSet, bool deleteExistFile)
        {
            AssertXlsxExtension(fileName);
            if (deleteExistFile && File.Exists(fileName)) File.Delete(fileName);

            if (dataSet == null || dataSet.Tables.Count == 0) return;

            var workbook = OpenOrCreateXssf(fileName);

            foreach (DataTable dt in dataSet.Tables)
            {
                var sheet = GetOrCreateSheet(workbook, dt.TableName);

                // header row
                var headerRow = sheet.GetRow(0) ?? sheet.CreateRow(0);
                for (int c = 0; c < dt.Columns.Count; c++)
                    SetCellValue(headerRow.GetCell(c) ?? headerRow.CreateCell(c), dt.Columns[c].ColumnName);

                // data rows
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    var row = sheet.GetRow(r + 1) ?? sheet.CreateRow(r + 1);
                    for (int c = 0; c < dt.Columns.Count; c++)
                        SetCellValue(row.GetCell(c) ?? row.CreateCell(c), dt.Rows[r][c]);
                }
            }

            SaveXssf(workbook, fileName);
        }

        /// <summary>
        /// Writes a single DataTable to an .xlsx file.
        /// </summary>
        public static void writeWorkbook(string fileName, DataTable dataTable)
        {
            writeWorkbook(fileName, dataTable, false);
        }

        /// <summary>
        /// Writes a single DataTable to an .xlsx file, optionally deleting any existing file first.
        /// </summary>
        public static void writeWorkbook(string fileName, DataTable dataTable, bool deleteExistFile)
        {
            var ds = new DataSet();
            ds.Tables.Add(dataTable.Copy());
            writeWorkbook(fileName, ds, deleteExistFile);
        }
    }
}
