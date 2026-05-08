# Memory notes from Excel read port

- Read APIs (getSheetData, getExcelSheetNames, getWorkbookSheetDatasets) already use NPOI (XSSFWorkbook/HSSFWorkbook) and ExcelDataReader for .xlsb.
- Tests under tests/analyticsLibrary.Excel.Tests use NPOI in-memory workbooks; dotnet test passed (72 tests).
- getSheetData and getWorkbookSheetDatasets build DataTable per worksheet. For .xlsx, XSSFSheet.GetTables() is used to enumerate structured tables; BuildDataTableFromNpoiSheet consumes table start/end row/col indices and duplicates table names are suffixed on the same sheet.
- EPPlus types (ExcelPackage, ExcelWorksheet) remain used in write methods; full EPPlus removal requires porting write paths before removing the PackageReference from analyticsLibrary.Excel.csproj.
- analyticsLibrary.Excel.csproj still contains a PackageReference to EPPlus. Read-only work is complete for the read APIs, but writes still depend on EPPlus and must be ported in a follow-up.

