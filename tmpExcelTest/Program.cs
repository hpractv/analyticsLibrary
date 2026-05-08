using System;
using System.IO;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using analyticsLibrary.Excel;

class Program
{
    static void Main()
    {
        var workbook = new XSSFWorkbook();
        var sheetWithTable = workbook.CreateSheet("WithTable");
        var headerRow = sheetWithTable.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("ColA");
        headerRow.CreateCell(1).SetCellValue("ColB");
        var r1 = sheetWithTable.CreateRow(1);
        r1.CreateCell(0).SetCellValue(1);
        r1.CreateCell(1).SetCellValue("A");
        var r2 = sheetWithTable.CreateRow(2);
        r2.CreateCell(0).SetCellValue(2);
        r2.CreateCell(1).SetCellValue("B");
        try {
            var xssfSheet = sheetWithTable as XSSFSheet;
            if (xssfSheet != null)
            {
                var table = xssfSheet.CreateTable();
                table.CellReferences = new NPOI.SS.Util.AreaReference("A1:B3", NPOI.SS.SpreadsheetVersion.EXCEL2007);
                table.Name = "MyTable";
            }
        }
        catch(Exception ex) {
            Console.WriteLine("Table creation exception: " + ex.Message);
        }

        var sheetNoTable = workbook.CreateSheet("NoTable");
        var h2 = sheetNoTable.CreateRow(0);
        h2.CreateCell(0).SetCellValue("X");
        h2.CreateCell(1).SetCellValue("Y");
        var n1 = sheetNoTable.CreateRow(1);
        n1.CreateCell(0).SetCellValue(10);
        n1.CreateCell(1).SetCellValue("Z");

        var fname = "test.xlsx";
        using (var fs = File.Create(fname))
        {
            workbook.Write(fs);
        }

        using (var fs2 = File.OpenRead(fname))
        {
            var datasets = excelLibrary.getWorkbookSheetDatasets(fs2, ".xlsx");
            Console.WriteLine("Returned datasets: " + datasets.Length);
            for (int i=0; i<datasets.Length; i++){
                var ds = datasets[i];
                Console.WriteLine($"DataSet {i} Name: {ds.DataSetName}, Tables: {ds.Tables.Count}");
                foreach (DataTable t in ds.Tables)
                {
                    Console.WriteLine($" Table {t.TableName}: Columns {t.Columns.Count}, Rows {t.Rows.Count}");
                }
            }
        }
    }
}
