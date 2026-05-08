# Memory notes from Excel read port

- Read APIs (getSheetData, getExcelSheetNames, getWorkbookSheetDatasets) already use NPOI (XSSFWorkbook/HSSFWorkbook) and ExcelDataReader for .xlsb.
- Tests under tests/analyticsLibrary.Excel.Tests use NPOI in-memory workbooks; dotnet test passed (72 tests).
- getSheetData and getWorkbookSheetDatasets build DataTable per worksheet. For .xlsx, XSSFSheet.GetTables() is used to enumerate structured tables; BuildDataTableFromNpoiSheet consumes table start/end row/col indices and duplicates table names are suffixed on the same sheet.
- EPPlus types (ExcelPackage, ExcelWorksheet) were removed from excelLibrary.cs; write APIs have been ported to NPOI (XSSFWorkbook/HSSFWorkbook). No OfficeOpenXml or System.Data.OleDb usages remain in src/analyticsLibrary.Excel.
- analyticsLibrary.Excel.csproj no longer references EPPlus or System.Data.OleDb; read and write now use NPOI and ExcelDataReader. Building the solution succeeds after the change.

- Note: A small cosmetic edit removed the literal "ExcelWorksheet" token from a write-API comment in excelLibrary.cs to ensure no accidental symbol references remain; change committed.

