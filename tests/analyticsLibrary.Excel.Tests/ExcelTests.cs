using System;
using System.Data;
using System.IO;
using analyticsLibrary.Excel;
using Xunit;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnLettersEnum_Values_A_Z_AA_AQ_AZ()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);
            Assert.Equal(52, Enum.GetValues(typeof(columnLettersEnum)).Length);
        }

        [Fact]
        public void ColumnNumber_Extension_Works()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttribute_Stores_Names()
        {
            var attr = new sheetColumnAttribute("ColA", "ColB");
            Assert.NotNull(attr.sheetColumnNames);
            Assert.Contains("ColA", attr.sheetColumnNames);
            Assert.Contains("ColB", attr.sheetColumnNames);
        }

        public class RowModel
        {
            [sheetColumnAttribute("ColA")]
            public string ColA { get; set; }
        }

        [Fact]
        public void RowValue_Returns_Value_And_Throws_When_Missing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "valueA";
            dt.Rows.Add(row);

            var value = dt.Rows[0].rowValue<RowModel, string>("ColA");
            Assert.Equal("valueA", value);

            var dt2 = new DataTable();
            dt2.Columns.Add("Other", typeof(string));
            var row2 = dt2.NewRow();
            row2["Other"] = "x";
            dt2.Rows.Add(row2);

            Assert.Throws<extensions.columnNotFoundException>(() => dt2.Rows[0].rowValue<RowModel, string>("ColA"));
        }

        [Fact]
        public void Xssf_Workbook_RoundTrip_InMemory()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("hello");

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                using (var fs2 = File.OpenRead(temp))
                {
                    var read = new XSSFWorkbook(fs2);
                    var v = read.GetSheetAt(0).GetRow(1).GetCell(0).ToString();
                    Assert.Equal("hello", v);
                }
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public void GetExcelSheetName_Replaces_Spaces_With_Underscore()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet";
            var result = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet", result);

            var dt2 = new DataTable();
            dt2.TableName = "Name";
            dt2.ExtendedProperties["TableName"] = "Other Name";
            Assert.Equal("Other_Name", excelLibrary.getExcelSheetName(dt2));
        }

        [Fact]
        public void GetWorkbookSheetDatasets_InMemory_Xlsx_With_Table_And_Fallback()
        {
            var workbook = new XSSFWorkbook();
            var sheetWithTable = workbook.CreateSheet("WithTable");
            var headerRow = sheetWithTable.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ColA");
            headerRow.CreateCell(1).SetCellValue("ColB");
            var r1 = sheetWithTable.CreateRow(1);
            r1.CreateCell(0).SetCellValue(1);
            r1.CreateCell(1).SetCellValue("A");
            var r2 = sheetWithTable.CreateRow(2);
            r2.CreateCell(0).SetCellValue(2);
            r2.CreateCell(1).SetCellValue("B");
            var xssfSheet = sheetWithTable as XSSFSheet;
            if (xssfSheet != null)
            {
                var table = xssfSheet.CreateTable();
                table.CellReferences = new NPOI.SS.Util.AreaReference("A1:B3", NPOI.SS.SpreadsheetVersion.EXCEL2007);
                table.Name = "MyTable";
            }

            var sheetNoTable = workbook.CreateSheet("NoTable");
            var h2 = sheetNoTable.CreateRow(0);
            h2.CreateCell(0).SetCellValue("X");
            h2.CreateCell(1).SetCellValue("Y");
            var n1 = sheetNoTable.CreateRow(1);
            n1.CreateCell(0).SetCellValue(10);
            n1.CreateCell(1).SetCellValue("Z");

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                using (var fs2 = File.OpenRead(temp))
                {
                    var datasets = excelLibrary.getWorkbookSheetDatasets(fs2, ".xlsx");
                    Assert.Equal(2, datasets.Length);

                    var ds1 = datasets[0];
                    Assert.Equal("WithTable", ds1.DataSetName);
                    Assert.Equal(1, ds1.Tables.Count);
                    var t1 = ds1.Tables[0];
                    Assert.Equal("MyTable", t1.TableName);
                    Assert.Equal(2, t1.Columns.Count);
                    Assert.Equal(2, t1.Rows.Count);

                    var ds2 = datasets[1];
                    Assert.Equal("NoTable", ds2.DataSetName);
                    Assert.Equal(1, ds2.Tables.Count);
                    var t2 = ds2.Tables[0];
                    Assert.Equal("NoTable", t2.TableName);
                    Assert.Equal(2, t2.Columns.Count);
                    Assert.Equal(1, t2.Rows.Count);
                }
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public void GetSheetData_From_Xlsx_File_Works()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("hello");

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                var ds = excelLibrary.getSheetData(temp);
                Assert.NotNull(ds);
                Assert.Equal(1, ds.Tables.Count);
                var dt = ds.Tables[0];
                Assert.Equal("Sheet1", dt.TableName);
                Assert.Equal("ColA", dt.Columns[0].ColumnName);
                Assert.Equal("hello", dt.Rows[0][0].ToString());
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public void GetSheetData_From_Xls_File_Works()
        {
            var workbook = new NPOI.HSSF.UserModel.HSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("hello");

            var temp = Path.GetTempFileName() + ".xls";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                var ds = excelLibrary.getSheetData(temp);
                Assert.NotNull(ds);
                Assert.Equal(1, ds.Tables.Count);
                var dt = ds.Tables[0];
                Assert.Equal("Sheet1", dt.TableName);
                Assert.Equal("ColA", dt.Columns[0].ColumnName);
                Assert.Equal("hello", dt.Rows[0][0].ToString());
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public void GetExcelSheetNames_Returns_Sheet_Names_Xlsx()
        {
            var workbook = new XSSFWorkbook();
            workbook.CreateSheet("A");
            workbook.CreateSheet("B");
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp)) { workbook.Write(fs); }
                var names = excelLibrary.getExcelSheetNames(temp);
                Assert.Equal(2, names.Length);
                Assert.Equal("A", names[0]);
                Assert.Equal("B", names[1]);
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void WriteWorkbook_DataSet_CreatesValidXlsx()
        {
            var ds = new DataSet();
            var dt1 = new DataTable("Alpha");
            dt1.Columns.Add("Id"); dt1.Columns.Add("Name");
            dt1.Rows.Add("1", "Alice");
            dt1.Rows.Add("2", "Bob");
            var dt2 = new DataTable("Beta");
            dt2.Columns.Add("X");
            dt2.Rows.Add("hello");
            ds.Tables.Add(dt1); ds.Tables.Add(dt2);

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                excelLibrary.writeWorkbook(temp, ds, true);

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    Assert.Equal(2, wb.NumberOfSheets);
                    var s1 = wb.GetSheet("Alpha");
                    Assert.NotNull(s1);
                    Assert.Equal("Id", s1.GetRow(0).GetCell(0).StringCellValue);
                    Assert.Equal("Alice", s1.GetRow(1).GetCell(1).StringCellValue);
                    var s2 = wb.GetSheet("Beta");
                    Assert.NotNull(s2);
                    Assert.Equal("hello", s2.GetRow(1).GetCell(0).StringCellValue);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void WriteWorkbook_DataTable_CreatesValidXlsx()
        {
            var dt = new DataTable("Sheet1");
            dt.Columns.Add("Col1"); dt.Columns.Add("Col2");
            dt.Rows.Add("A", "B");

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                excelLibrary.writeWorkbook(temp, dt, true);

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    var sheet = wb.GetSheet("Sheet1");
                    Assert.NotNull(sheet);
                    Assert.Equal("Col1", sheet.GetRow(0).GetCell(0).StringCellValue);
                    Assert.Equal("A", sheet.GetRow(1).GetCell(0).StringCellValue);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void WriteSheetDataXlsx_Array_WritesExpectedCells()
        {
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                var data = new object[,] { { "H1", "H2" }, { 1, "A" }, { 2, "B" } };
                excelLibrary.writeSheetDataXlsx(temp, "Data", data, true);

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    var sheet = wb.GetSheet("Data");
                    Assert.NotNull(sheet);
                    Assert.Equal("H1", sheet.GetRow(0).GetCell(0).StringCellValue);
                    Assert.Equal("H2", sheet.GetRow(0).GetCell(1).StringCellValue);
                    // numeric cell
                    Assert.Equal(1.0, sheet.GetRow(1).GetCell(0).NumericCellValue);
                    Assert.Equal("A", sheet.GetRow(1).GetCell(1).StringCellValue);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        public class SimpleDto
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        [Fact]
        public void WriteSheetDataXlsx_Enumerable_WritesHeaderAndRows()
        {
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                var items = new[] { new SimpleDto { Name = "foo", Value = 42 }, new SimpleDto { Name = "bar", Value = 7 } };
                excelLibrary.writeSheetDataXlsx(temp, "Dto", items);

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    var sheet = wb.GetSheet("Dto");
                    Assert.NotNull(sheet);
                    // header row: property names (order from getColumnProperties)
                    var h0 = sheet.GetRow(0).GetCell(0).StringCellValue;
                    var h1 = sheet.GetRow(0).GetCell(1).StringCellValue;
                    Assert.True(h0 == "Name" || h0 == "Value");
                    Assert.True(h1 == "Name" || h1 == "Value");
                    // 2 data rows
                    Assert.NotNull(sheet.GetRow(1));
                    Assert.NotNull(sheet.GetRow(2));
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void CopySheet_DuplicatesSheetContent()
        {
            var workbook = new XSSFWorkbook();
            var src = workbook.CreateSheet("Original");
            src.CreateRow(0).CreateCell(0).SetCellValue("hello");
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp)) workbook.Write(fs);

                excelLibrary.copySheet(temp, "Original", "Copy");

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    Assert.Equal(2, wb.NumberOfSheets);
                    Assert.Equal("hello", wb.GetSheet("Copy").GetRow(0).GetCell(0).StringCellValue);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void RemoveWorksheet_ByName_RemovesSheet()
        {
            var workbook = new XSSFWorkbook();
            workbook.CreateSheet("Keep");
            workbook.CreateSheet("Remove");
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp)) workbook.Write(fs);

                excelLibrary.removeWorksheet(temp, "Remove");

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    Assert.Equal(1, wb.NumberOfSheets);
                    Assert.Equal("Keep", wb.GetSheetAt(0).SheetName);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void RemoveWorksheet_ByIndex_RemovesSheet()
        {
            var workbook = new XSSFWorkbook();
            workbook.CreateSheet("First");
            workbook.CreateSheet("Second");
            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp)) workbook.Write(fs);

                excelLibrary.removeWorksheet(temp, 1);

                using (var fs = File.OpenRead(temp))
                {
                    var wb = new XSSFWorkbook(fs);
                    Assert.Equal(1, wb.NumberOfSheets);
                    Assert.Equal("First", wb.GetSheetAt(0).SheetName);
                }
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void GetWorkbookSheetDatasets_Xls_ReturnsSheets()
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("XlsSheet");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            header.CreateCell(1).SetCellValue("ColB");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("v1");
            row.CreateCell(1).SetCellValue("v2");

            var temp = Path.GetTempFileName() + ".xls";
            try
            {
                using (var fs = File.Create(temp)) workbook.Write(fs);

                var datasets = excelLibrary.getWorkbookSheetDatasets(temp);
                Assert.Equal(1, datasets.Length);
                Assert.Equal("XlsSheet", datasets[0].DataSetName);
                Assert.Equal(1, datasets[0].Tables.Count);
                var dt = datasets[0].Tables[0];
                Assert.Equal(2, dt.Columns.Count);
                Assert.Equal(1, dt.Rows.Count);
                Assert.Equal("v1", dt.Rows[0][0].ToString());
            }
            finally { try { File.Delete(temp); } catch { } }
        }

        [Fact]
        public void WriteSheetDataXlsx_Xlsb_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                excelLibrary.writeSheetDataXlsx("/tmp/test.xlsb", "Sheet1", new object[,] { { "x" } }));
        }
    }
}
