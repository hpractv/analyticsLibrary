using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Xunit;
using analyticsLibrary.Excel;
using System.Data;

namespace analyticsLibrary.Excel.Tests
{
    public class ExcelWorkbookDatasetsSmokeTest
    {
        [Fact]
        public void XssfInMemory_WorkbookWithTableAndSheetWithoutTable_ReturnsTwoDataSets()
        {
            var wb = new XSSFWorkbook();
            var sheet1 = (XSSFSheet)wb.CreateSheet("WithTable");
            var header = sheet1.CreateRow(0);
            header.CreateCell(0).SetCellValue("ColA");
            header.CreateCell(1).SetCellValue("ColB");
            var dataRow = sheet1.CreateRow(1);
            dataRow.CreateCell(0).SetCellValue("1");
            dataRow.CreateCell(1).SetCellValue("2");

            var table = sheet1.CreateTable();
            var ctTable = table.GetCTTable();
            ctTable.id = 1U;
            ctTable.name = "MyTable";
            ctTable.displayName = "MyTable";
            ctTable.@ref = "A1:B2";
            table.DisplayName = "MyTable";
            table.Name = "MyTable";
            table.SetCellReferences(new NPOI.SS.Util.AreaReference("A1:B2", NPOI.SS.SpreadsheetVersion.EXCEL2007));

            var sheet2 = wb.CreateSheet("NoTable");
            var h2 = sheet2.CreateRow(0);
            h2.CreateCell(0).SetCellValue("X");
            h2.CreateCell(1).SetCellValue("Y");
            var d2 = sheet2.CreateRow(1);
            d2.CreateCell(0).SetCellValue("a");
            d2.CreateCell(1).SetCellValue("b");

            using (var ms = new MemoryStream())
            {
                wb.Write(ms);
                ms.Position = 0;
                var datasets = excelLibrary.getWorkbookSheetDatasets(ms, ".xlsx");
                Assert.Equal(2, datasets.Length);

                var ds1 = datasets[0];
                Assert.Equal("WithTable", ds1.DataSetName);
                Assert.Equal(1, ds1.Tables.Count);
                Assert.Equal("MyTable", ds1.Tables[0].TableName);
                Assert.Equal("ColA", ds1.Tables[0].Columns[0].ColumnName);

                var ds2 = datasets[1];
                Assert.Equal("NoTable", ds2.DataSetName);
                Assert.Equal(1, ds2.Tables.Count);
                Assert.Equal("X", ds2.Tables[0].Columns[0].ColumnName);
            }
        }
    }
}
