using System;
using System.Data;
using System.Linq;
using OfficeOpenXml;
using Xunit;
using analyticsLibrary.Excel;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnLettersEnumValues()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);

            var values = Enum.GetValues(typeof(columnLettersEnum)).Cast<int>().ToArray();
            Assert.Equal(52, values.Length);
            Assert.Equal(52, values.Distinct().Count());
        }

        [Fact]
        public void ColumnNumberExtension()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttributeStoresNames()
        {
            var attr = new sheetColumnAttribute("ColA", "ColB");
            Assert.NotNull(attr.sheetColumnNames);
            Assert.Equal(new[] { "ColA", "ColB" }, attr.sheetColumnNames);
        }

        class ModelWithAttribute
        {
            [sheetColumnAttribute("ColA")]
            public string Name { get; set; }
        }

        [Fact]
        public void RowValueReturnsValueAndThrowsWhenMissing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var r = dt.NewRow();
            r["ColA"] = "Hello";
            dt.Rows.Add(r);

            var val = dt.Rows[0].rowValue<ModelWithAttribute, string>("Name");
            Assert.Equal("Hello", val);

            var dt2 = new DataTable();
            dt2.Columns.Add("Other", typeof(string));
            var row2 = dt2.NewRow();
            row2["Other"] = "x";
            dt2.Rows.Add(row2);

            Assert.Throws<analyticsLibrary.Excel.extensions.columnNotFoundException>(() => row2.rowValue<ModelWithAttribute, string>("Name"));
        }

        [Fact]
        public void EPPlusWorkbookRoundTrip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "test123";
                using (var ms = new System.IO.MemoryStream())
                {
                    package.SaveAs(ms);
                    ms.Position = 0;
                    using (var package2 = new ExcelPackage(ms))
                    {
                        var value = package2.Workbook.Worksheets[0].Cells[1, 1].Text;
                        Assert.Equal("test123", value);
                    }
                }
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpacesWithUnderscore()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet";
            var name = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet", name);
            dt.ExtendedProperties["TableName"] = "Other Name";
            Assert.Equal("Other_Name", excelLibrary.getExcelSheetName(dt));
        }
    }
}
