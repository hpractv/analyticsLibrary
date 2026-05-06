using System;
using Xunit;
using analyticsLibrary.Excel;
using OfficeOpenXml;
using System.Data;
using System.Linq;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        public ExcelTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public void ColumnLettersEnum_Values_A_Z_AA_AQ_AZ()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);
            var vals = Enum.GetValues(typeof(columnLettersEnum)).Cast<int>().Distinct();
            Assert.Equal(52, vals.Count());
        }

        [Fact]
        public void Extensions_ColumnNumber_Returns_Int()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttribute_Stores_Names()
        {
            var attr = new sheetColumnAttribute("ColA", "ColB");
            Assert.Equal(new[] {"ColA","ColB"}, attr.sheetColumnNames);
        }

        private class ModelWithColumn
        {
            [sheetColumnAttribute("ColA")]
            public string MyProp { get; set; }
        }

        [Fact]
        public void RowValue_Returns_Value_And_Throws_When_Missing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "ValueA";
            dt.Rows.Add(row);

            var val = row.rowValue<ModelWithColumn, string>("MyProp");
            Assert.Equal("ValueA", val);

            var dt2 = new DataTable();
            dt2.Columns.Add("Other", typeof(string));
            var row2 = dt2.NewRow(); row2["Other"] = "X"; dt2.Rows.Add(row2);
            Assert.Throws<extensions.columnNotFoundException>(() => row2.rowValue<ModelWithColumn, string>("MyProp"));
        }

        [Fact]
        public void EPPlus_Workbook_RoundTrip_InMemory()
        {
            using (var pkg = new ExcelPackage())
            {
                var wb = pkg.Workbook;
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cells[1,1].Value = "Hello";
                Assert.Equal("Hello", ws.Cells[1,1].Text);
            }
        }

        [Fact]
        public void GetExcelSheetName_Replaces_Spaces_With_Underscore()
        {
            var dt = new DataTable("My Table");
            Assert.Equal("My_Table", excelLibrary.getExcelSheetName(dt));
            dt.TableName = "Another Name";
            dt.ExtendedProperties["TableName"] = "FromProp Name";
            Assert.Equal("FromProp_Name", excelLibrary.getExcelSheetName(dt));
        }
    }
}