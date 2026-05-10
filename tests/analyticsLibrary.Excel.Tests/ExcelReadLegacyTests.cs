using System;
using System.Data;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Xunit;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelReadLegacyTests
    {
        [Fact]
        public void GetSheetData_InMemory_Xlsx()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("hello-xlsx");

            var temp = Path.GetTempFileName() + ".xlsx";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                var ds = analyticsLibrary.Excel.excelLibrary.getSheetData(temp);
                Assert.NotNull(ds);
                Assert.Equal(1, ds.Tables.Count);
                var t = ds.Tables[0];
                Assert.Equal("Sheet1", t.TableName);
                Assert.Equal("hello-xlsx", t.Rows[0][0].ToString());
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public void GetSheetData_InMemory_Xls()
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("OldSheet");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            var row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("hello-xls");

            var temp = Path.GetTempFileName() + ".xls";
            try
            {
                using (var fs = File.Create(temp))
                {
                    workbook.Write(fs);
                }

                var ds = analyticsLibrary.Excel.excelLibrary.getSheetData(temp);
                Assert.NotNull(ds);
                Assert.Equal(1, ds.Tables.Count);
                var t = ds.Tables[0];
                Assert.Equal("OldSheet", t.TableName);
                Assert.Equal("hello-xls", t.Rows[0][0].ToString());
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }
    }
}
