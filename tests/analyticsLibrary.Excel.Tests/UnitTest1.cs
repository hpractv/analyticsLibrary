using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using analyticsLibrary.Excel;
using OfficeOpenXml;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnLetters_ValuesAndCount()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);

            var values = Enum.GetValues(typeof(columnLettersEnum)).Cast<columnLettersEnum>().Select(v => (int)v).ToArray();
            Assert.Equal(52, values.Length);
            Assert.Equal(52, values.Distinct().Count());
        }

        [Fact]
        public void ColumnNumber_Extension()
        {
            Assert.Equal(1, columnLettersEnum.A.columnNumber());
            Assert.Equal(26, columnLettersEnum.Z.columnNumber());
            Assert.Equal(27, columnLettersEnum.AA.columnNumber());
        }

        [Fact]
        public void SheetColumnAttribute_StoresNames()
        {
            var attr = new sheetColumnAttribute("ColA", "ColB");
            Assert.Equal(new[] { "ColA", "ColB" }, attr.sheetColumnNames);
        }

        public class TestModel
        {
            [sheetColumnAttribute("ColA")]
            public string Value { get; set; }

            [sheetColumnAttribute("MissingCol")]
            public string MissingProp { get; set; }
        }

        [Fact]
        public void RowValue_ReturnsValue_AndThrowsWhenMissing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "hello";
            dt.Rows.Add(row);

            var actual = dt.Rows[0].rowValue<TestModel>(nameof(TestModel.Value));
            Assert.Equal("hello", actual);

            // missing column should throw
            var ex = Assert.Throws<extensions.columnNotFoundException>(() => dt.Rows[0].rowValue<TestModel>(nameof(TestModel.MissingProp)));
        }

        [Fact]
        public void EPPlus_Workbook_RoundTrip_InMemory()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage())
            {
                var ws = p.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "X";
                using (var ms = new MemoryStream())
                {
                    p.SaveAs(ms);
                    ms.Position = 0;
                    using (var p2 = new ExcelPackage(ms))
                    {
                        var v = p2.Workbook.Worksheets[0].Cells[1, 1].Text;
                        Assert.Equal("X", v);
                    }
                }
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpacesWithUnderscores()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet";
            var name = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("My_Sheet", name);

            dt.ExtendedProperties["TableName"] = "Another Name";
            var name2 = excelLibrary.getExcelSheetName(dt);
            Assert.Equal("Another_Name", name2);
        }
    }
}
