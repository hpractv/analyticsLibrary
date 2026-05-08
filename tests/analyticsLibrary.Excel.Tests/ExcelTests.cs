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
    }
}
