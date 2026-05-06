using System;
using System.Data;
using System.IO;
using System.Linq;
using analyticsLibrary.Excel;
using OfficeOpenXml;
using Xunit;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnEnumValuesAndCount()
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
        public void ColumnNumberExtensionReturnsInt()
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
            Assert.Equal(2, attr.sheetColumnNames.Length);
            Assert.Contains("ColA", attr.sheetColumnNames);
            Assert.Contains("ColB", attr.sheetColumnNames);
        }

        public class ModelWithAttr
        {
            [sheetColumnAttribute("ColA")]
            public string Value { get; set; }
        }

        [Fact]
        public void RowValueReturnsValueAndThrowsOnMissing()
        {
            var table = new DataTable();
            table.Columns.Add("ColA", typeof(string));
            var row = table.NewRow();
            row["ColA"] = "Hello";
            table.Rows.Add(row);

            var result = table.Rows[0].rowValue<ModelWithAttr, string>("Value");
            Assert.Equal("Hello", result);

            var emptyTable = new DataTable();
            emptyTable.Columns.Add("Other", typeof(string));
            var emptyRow = emptyTable.NewRow();
            emptyTable.Rows.Add(emptyRow);

            Assert.Throws<analyticsLibrary.Excel.extensions.columnNotFoundException>(() => emptyTable.Rows[0].rowValue<ModelWithAttr, string>("Value"));
        }

        [Fact]
        public void EPPlus_InMemory_RoundTrip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "TestValue";
                using (var ms = new MemoryStream())
                {
                    package.SaveAs(ms);
                    ms.Position = 0;
                    using (var p2 = new ExcelPackage(ms))
                    {
                        var value = p2.Workbook.Worksheets[0].Cells[1,1].Text;
                        Assert.Equal("TestValue", value);
                    }
                }
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpacesWithUnderscores()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet";
            Assert.Equal("My_Sheet", excelLibrary.getExcelSheetName(dt));

            var dt2 = new DataTable();
            dt2.ExtendedProperties["TableName"] = "Another Sheet";
            Assert.Equal("Another_Sheet", excelLibrary.getExcelSheetName(dt2));
        }
    }
}
