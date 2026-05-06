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
        public void ColumnLettersEnum_Values_AreCorrect()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);
        }

        [Fact]
        public void ColumnLettersEnum_Has52UniqueValues()
        {
            var values = Enum.GetValues(typeof(columnLettersEnum)).Cast<int>().ToArray();
            Assert.Equal(52, values.Length);
            Assert.Equal(52, values.Distinct().Count());
        }

        [Fact]
        public void ColumnNumberExtension_ReturnsCorrectNumber()
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
            Assert.Equal(2, attr.sheetColumnNames.Length);
            Assert.Contains("ColA", attr.sheetColumnNames);
            Assert.Contains("ColB", attr.sheetColumnNames);
        }

        class Model
        {
            [sheetColumnAttribute("ColA")]
            public string PropA { get; set; }

            [sheetColumnAttribute("Other")]
            public string PropOther { get; set; }
        }

        [Fact]
        public void RowValue_ReturnsValue_ForDecoratedColumn()
        {
            var dt = new DataTable();
            dt.TableName = "Test";
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "value";
            dt.Rows.Add(row);

            var result = dt.Rows[0].rowValue<Model>("PropA");
            Assert.Equal("value", result);
        }

        [Fact]
        public void RowValue_Throws_WhenColumnMissing()
        {
            var dt = new DataTable();
            dt.TableName = "Test";
            dt.Columns.Add("NotCol", typeof(string));
            var row = dt.NewRow();
            row["NotCol"] = "x";
            dt.Rows.Add(row);
            Assert.Throws<extensions.columnNotFoundException>(() => dt.Rows[0].rowValue<Model>("PropA"));
        }

        [Fact]
        public void EPPlus_RoundTrip_Works()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1,1].Value = "Hello";
                var cell = ws.Cells[1,1].Value;
                Assert.Equal("Hello", cell);
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpacesWithUnderscores()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet Name";
            var ret = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet_Name", ret);
        }
    }
}
