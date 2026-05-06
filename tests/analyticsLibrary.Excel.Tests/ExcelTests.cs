using System;
using System.Data;
using System.IO;
using OfficeOpenXml;
using analyticsLibrary.Excel;
using Xunit;

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
        public void EPPlus_Workbook_RoundTrip_InMemory()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var ms = new MemoryStream())
            {
                using (var p = new ExcelPackage())
                {
                    var ws = p.Workbook.Worksheets.Add("Sheet1");
                    ws.Cells[1, 1].Value = "hello";
                    p.SaveAs(ms);
                }

                ms.Position = 0;
                using (var p2 = new ExcelPackage(ms))
                {
                    var v = p2.Workbook.Worksheets[0].Cells[1, 1].Text;
                    Assert.Equal("hello", v);
                }
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
    }
}
