using System;
using System.Data;
using System.IO;
using Xunit;
using analyticsLibrary.Excel;
using OfficeOpenXml;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelTests
    {
        [Fact]
        public void ColumnLettersEnum_ValuesAndCount()
        {
            Assert.Equal(1, (int)columnLettersEnum.A);
            Assert.Equal(26, (int)columnLettersEnum.Z);
            Assert.Equal(27, (int)columnLettersEnum.AA);
            Assert.Equal(43, (int)columnLettersEnum.AQ);
            Assert.Equal(52, (int)columnLettersEnum.AZ);
            var values = Enum.GetValues(typeof(columnLettersEnum));
            Assert.Equal(52, values.Length);
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
            var attr = new sheetColumnAttribute("ColA","ColB");
            Assert.NotNull(attr.sheetColumnNames);
            Assert.Equal(new[] { "ColA", "ColB" }, attr.sheetColumnNames);
        }

        class ModelWithAttr
        {
            [sheetColumnAttribute("ColA")]
            public string MyProp { get; set; }
        }

        [Fact]
        public void RowValue_ReturnsValue_AndThrowsWhenMissing()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColA", typeof(string));
            var row = dt.NewRow();
            row["ColA"] = "hello";
            dt.Rows.Add(row);
            var dr = dt.Rows[0];
            var val = dr.rowValue<ModelWithAttr, string>("MyProp");
            Assert.Equal("hello", val);

            var dt2 = new DataTable();
            dt2.Columns.Add("Other", typeof(string));
            var row2 = dt2.NewRow();
            row2["Other"] = "x";
            dt2.Rows.Add(row2);
            var dr2 = dt2.Rows[0];
            Assert.Throws<analyticsLibrary.Excel.extensions.columnNotFoundException>(() => dr2.rowValue<ModelWithAttr, string>("MyProp"));
        }

        [Fact]
        public void EPPlus_InMemory_RoundTrip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var ms = new MemoryStream())
            using (var package = new ExcelPackage(ms))
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells[1, 1].Value = "test-value";
                package.Save();
                ms.Position = 0;
                using (var package2 = new ExcelPackage(ms))
                {
                    var ws2 = package2.Workbook.Worksheets["Sheet1"];
                    Assert.Equal("test-value", ws2.Cells[1, 1].GetValue<string>());
                }
            }
        }

        [Fact]
        public void GetExcelSheetName_ReplacesSpacesWithUnderscore()
        {
            var dt = new DataTable();
            dt.TableName = "My Sheet Name";
            var type = typeof(columnLettersEnum).Assembly.GetType("analyticsLibrary.Excel.excelLibrary");
            Assert.NotNull(type);
            var method = type.GetMethod("getExcelSheetName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            var result = method.Invoke(null, new object[] { dt }) as string;
            Assert.Equal("My_Sheet_Name", result);
        }
    }
}
