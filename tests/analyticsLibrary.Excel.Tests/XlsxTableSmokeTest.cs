using System;
using System.IO;
using System.Linq;
using Xunit;
using OfficeOpenXml;

namespace analyticsLibrary.Excel.Tests
{
    public class XlsxTableSmokeTest
    {
        [Fact]
        public void GetWorkbookSheetDatasets_Returns_Tables_And_Fallback()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage())
            {
                var wsTable = p.Workbook.Worksheets.Add("TableSheet");
                wsTable.Cells[1, 1].Value = "ColA";
                wsTable.Cells[1, 2].Value = "ColB";
                wsTable.Cells[2, 1].Value = "r1c1";
                wsTable.Cells[2, 2].Value = "r1c2";
                var tblRange = wsTable.Cells[1, 1, 2, 2];
                wsTable.Tables.Add(tblRange, "MyTable");

                var wsNo = p.Workbook.Worksheets.Add("NoTable");
                wsNo.Cells[1, 1].Value = "H1";
                wsNo.Cells[1, 2].Value = "H2";
                wsNo.Cells[2, 1].Value = "a";
                wsNo.Cells[2, 2].Value = "b";

                using (var ms = new MemoryStream())
                {
                    p.SaveAs(ms);
                    ms.Position = 0;

                    var datasets = excelLibrary.getWorkbookSheetDatasets(ms, ".xlsx");
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
        }
    }
}
