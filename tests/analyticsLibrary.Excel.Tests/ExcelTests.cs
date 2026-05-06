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
        public void ColumnLettersEnum_Values_A_Z_AA_AQ_AZ()
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
        public void ColumnNumberExtension_Works()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttribute_StoresNames()
        {
            var attr = new sheetColumnAttribute("ColA", "ColB");
            Assert.NotNull(attr.sheetColumnNames);
            Assert.Contains("ColA", attr.sheetColumnNames);
            Assert.Contains("ColB", attr.sheetColumnNames);
        }

        private class ModelWithColumn
        {
            [sheetColumnAttribute("ColA")]
            public string MyProp { get; set; }

            public string MissingProp { get; set; }
        }

        [Fact]
        public void RowValue_Returns_Value_And_Throws_When_Missing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "valueA";
            dt.Rows.Add(row);

            // successful retrieval
            var val = row.rowValue<ModelWithColumn, string>(nameof(ModelWithColumn.MyProp));
            Assert.Equal("valueA", val);

            // missing column should throw columnNotFoundException
            var ex = Assert.Throws<extensions.columnNotFoundException>(() => row.rowValue<ModelWithColumn, string>(nameof(ModelWithColumn.MissingProp)));
            Assert.NotNull(ex);
        }

        [Fact]
        public void EPPlus_Workbook_RoundTrip_InMemory()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage())
            {
                var ws = p.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "HelloEPPlus";

                // read back
                Assert.Equal("HelloEPPlus", p.Workbook.Worksheets[0].Cells[1,1].Text);
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpaces()
        {
            var dt = new DataTable("My Sheet");
            var name = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet", name);
        }
    }
}
