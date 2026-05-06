using System;
using System.Data;
using System.IO;
using Xunit;
using OfficeOpenXml;
using analyticsLibrary.Excel;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnLettersEnum_Values_AreCorrect()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);
            Assert.Equal(52, Enum.GetValues(typeof(columnLettersEnum)).Length);
        }

        [Fact]
        public void ColumnLettersEnum_Count_ReturnsCorrectNumber()
        {
            var values = Enum.GetValues(typeof(columnLettersEnum));
            Assert.Equal(52, values.Length);
        }

        [Fact]
        public void ColumnNumberExtension_ReturnsInt()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttribute_StoresNames()
        {
            var attr = new sheetColumnAttribute("ColA","ColB");
            Assert.NotNull(attr.sheetColumnNames);
            Assert.Equal(new[] { "ColA", "ColB" }, attr.sheetColumnNames);
        }

        private class RowModel
        {
            [sheetColumnAttribute("ColA")]
            public string MyProp { get; set; }
        }

        [Fact]
        public void RowValue_Returns_CorrectValue_And_Throws_When_NotFound()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var r = dt.NewRow();
            r["ColA"] = "hello";
            dt.Rows.Add(r);

            var value = dt.Rows[0].rowValue<RowModel, string>(nameof(RowModel.MyProp));
            Assert.Equal("hello", value);

            var dt2 = new DataTable();
            dt2.Columns.Add("OtherCol", typeof(string));
            var r2 = dt2.NewRow();
            r2["OtherCol"] = "x";
            dt2.Rows.Add(r2);

            Assert.Throws<extensions.columnNotFoundException>(() => dt2.Rows[0].rowValue<RowModel, string>(nameof(RowModel.MyProp)));
        }

        [Fact]
        public void EPPlus_Workbook_RoundTrip_InMemory()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "test-value";

                // save to stream and reload
                using (var ms = new MemoryStream())
                {
                    package.SaveAs(ms);
                    ms.Position = 0;
                    using (var read = new ExcelPackage(ms))
                    {
                        var readWs = read.Workbook.Worksheets[0];
                        Assert.Equal("test-value", readWs.Cells[1, 1].Text);
                    }
                }
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpaces_WithUnderscore()
        {
            var dt = new DataTable("My Sheet");
            var name = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet", name);

            var dt2 = new DataTable("IgnoredName");
            dt2.ExtendedProperties.Add("TableName", "Sheet With Spaces");
            var name2 = excelLibrary.getExcelSheetName(dt2);
            Assert.Equal("Sheet_With_Spaces", name2);
        }
    }
}
