using System;
using System.IO;
using System.Linq;
using Xunit;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace analyticsLibrary.Excel.Tests
{
    public class XlsxTableSmokeTest
    {
        [Fact]
        public void GetWorkbookSheetDatasets_Returns_Tables_And_Fallback()
        {
            var workbook = new XSSFWorkbook();
            var sheetWithTable = workbook.CreateSheet("TableSheet");
            var headerRow = sheetWithTable.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ColA");
            headerRow.CreateCell(1).SetCellValue("ColB");
            var r1 = sheetWithTable.CreateRow(1);
            r1.CreateCell(0).SetCellValue("r1c1");
            r1.CreateCell(1).SetCellValue("r1c2");

            var xssfSheet = sheetWithTable as XSSFSheet;
            if (xssfSheet != null)
            {
                var table = xssfSheet.CreateTable();
                table.CellReferences = new AreaReference("A1:B2", NPOI.SS.SpreadsheetVersion.EXCEL2007);
                table.Name = "MyTable";
            }

            var sheetNoTable = workbook.CreateSheet("NoTable");
            var h2 = sheetNoTable.CreateRow(0);
            h2.CreateCell(0).SetCellValue("H1");
            h2.CreateCell(1).SetCellValue("H2");
            var n1 = sheetNoTable.CreateRow(1);
            n1.CreateCell(0).SetCellValue("a");
            n1.CreateCell(1).SetCellValue("b");

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

                    var dsTable = datasets.FirstOrDefault(d => d.DataSetName == "TableSheet");
                    Assert.NotNull(dsTable);
                    Assert.True(dsTable.Tables.Count >= 1);
                    var dt = dsTable.Tables[0];
                    Assert.Contains(dt.TableName, new[] { "MyTable", "TableSheet" });

                    var dsNo = datasets.FirstOrDefault(d => d.DataSetName == "NoTable");
                    Assert.NotNull(dsNo);
                    Assert.Single(dsNo.Tables);
                    var dtNo = dsNo.Tables[0];
                    Assert.Equal("NoTable", dtNo.TableName);
                    Assert.Equal("H1", dtNo.Columns[0].ColumnName);
                }
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }
    }
}
