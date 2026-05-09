# analyticsLibrary

[![NuGet](https://img.shields.io/nuget/v/analyticsLibrary.Core.svg?label=analyticsLibrary.Core)](https://www.nuget.org/packages/analyticsLibrary.Core/)
[![CI](https://github.com/hpractv/analyticsLibrary/actions/workflows/ci.yml/badge.svg)](https://github.com/hpractv/analyticsLibrary/actions/workflows/ci.yml)
[![Release](https://github.com/hpractv/analyticsLibrary/actions/workflows/release.yml/badge.svg)](https://github.com/hpractv/analyticsLibrary/actions/workflows/release.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)

A family of focused .NET 8 packages for reading and writing CSV and fixed-width files, performing statistical operations, running sort and search algorithms, and working with Excel, Access, and Hadoop data sources.

**Version 2.0.0** is a breaking-change release. The monolithic `analyticsLibrary` 1.x package has been split into focused packages. SQL Server, Oracle, SAS, and Sybase provider support has been removed. See the [Migration Guide](#migration-guide) below.

**`analyticsLibrary.Excel` 3.0.0** removes EPPlus (PolyForm NonCommercial) in favor of FOSS-only dependencies (NPOI Apache-2.0, ExcelDataReader MIT). This is a breaking change for callers using EPPlus-typed API surface. See [From analyticsLibrary.Excel 2.x to 3.0.0](#from-analyticslibraryexcel-2x-to-300) in the Migration Guide.

---

## Table of Contents

- [Package Overview](#package-overview)
- [Namespace Reference](#namespace-reference)
- [Installation](#installation)
- [Quick Starts](#quick-starts)
- [Functionality](#functionality)
- [Repository Layout](#repository-layout)
- [Build and Test](#build-and-test)
- [Release Process](#release-process)
- [Contributing](#contributing)
- [Migration Guide](#migration-guide)

---

## Package Overview

| Package | Purpose | Primary Namespace | Key Dependencies |
|---|---|---|---|
| `analyticsLibrary.Core` | CSV, fixed-width, shared data objects, helpers | `analyticsLibrary.Core` | none |
| `analyticsLibrary.Algorithms` | Merge sort, quick sort, search | `analyticsLibrary.Algorithms` | Core |
| `analyticsLibrary.Statistics` | Covariance, dot product, histogram, normalize, stddev, variance | `analyticsLibrary.Statistics` | Core |
| `analyticsLibrary.Excel` | Excel file read/write (.xlsx/.xls/.xlsb) via NPOI and ExcelDataReader | `analyticsLibrary.Excel` | Core, NPOI (Apache-2.0), ExcelDataReader (MIT) |
| `analyticsLibrary.Access` | Access database file support | `analyticsLibrary.Access` | Core, System.Data.OleDb (Windows) |
| `analyticsLibrary.Hadoop` | Hadoop/ODBC data access | `analyticsLibrary.Hadoop` | Core, System.Data.Odbc |
| `analyticsLibrary` | Transition metapackage (references all above) | — | all above |

> **Platform note**: `analyticsLibrary.Access` depends on `System.Data.OleDb`, which is Windows-only at runtime. `analyticsLibrary.Excel` uses NPOI and ExcelDataReader, which are cross-platform.

---

## Namespace Reference

| Namespace | Package | Contents |
|---|---|---|
| `analyticsLibrary.Core` | `analyticsLibrary.Core` | `csv`, `fixedWidthFile`, `data`, `table`, `column`, `dataTypeHelper`, `dataTypeEnum`, `extensions`, `analytics`, `loopDateRange`, `presentation`, `login`, `utility`, `indexObject`, `typedIndex` |
| `analyticsLibrary.Algorithms` | `analyticsLibrary.Algorithms` | `sorting` (merge sort, quick sort), `searching` |
| `analyticsLibrary.Statistics` | `analyticsLibrary.Statistics` | `extensions` (standardDeviation, variance, covariance, normalize, histogram, dot, norm) |
| `analyticsLibrary.Excel` | `analyticsLibrary.Excel` | `excelLibrary`, `extensions`, `columnLettersEnum`, `sheetColumnAttribute` |
| `analyticsLibrary.Access` | `analyticsLibrary.Access` | `access` |
| `analyticsLibrary.Hadoop` | `analyticsLibrary.Hadoop` | `hadoop` |

---

## Installation

Install only the packages you need for a leaner dependency graph.

```shell
# Core CSV and data helpers
dotnet add package analyticsLibrary.Core

# Sort and search algorithms
dotnet add package analyticsLibrary.Algorithms

# Statistics
dotnet add package analyticsLibrary.Statistics

# Excel file operations (cross-platform: .xlsx/.xls/.xlsb read; .xlsx/.xls write)
dotnet add package analyticsLibrary.Excel

# Access database support (Windows runtime only)
dotnet add package analyticsLibrary.Access

# Hadoop/ODBC support
dotnet add package analyticsLibrary.Hadoop

# Everything (transition metapackage)
dotnet add package analyticsLibrary
```

---

## Quick Starts

### Read a CSV file

```csharp
using analyticsLibrary.Core;

var file = new csv("data.csv");
foreach (var row in file.data)
{
    Console.WriteLine(string.Join(", ", row.values));
}
```

### Write a CSV file

```csharp
using analyticsLibrary.Core;

var header = new[] { "Name", "Score" };
var rows = new[]
{
    new object[] { "Alice", 95 },
    new object[] { "Bob",   87 },
};
rows.writeCsv("output.csv", header, ',');
```

### Standard deviation and normalization

```csharp
using analyticsLibrary.Statistics;

double[] scores = { 70, 80, 85, 90, 95 };
double stddev = scores.standardDeviation();
double[] normalized = scores.normalize();  // maps min to 0.0, max to 1.0
```

### Sort a list

```csharp
using analyticsLibrary.Algorithms;

int[] values = { 5, 3, 8, 1, 9 };
int[] sorted = values.mergeSort();           // ascending
int[] desc   = values.quickSort(descending: true);  // descending
```

### Parse a SAS date string

```csharp
using analyticsLibrary.Core;

DateTime? date = "01JAN2020:00:00:00.000".fromSasEpochDate();
```

---

## Functionality

### CSV and Fixed-Width Files (`analyticsLibrary.Core`)

- Read CSV files with configurable delimiters and optional headers via the `csv` class.
- Write `IEnumerable<object[]>` to CSV using `writeCsv` extension methods.
- Parse individual CSV lines with `fromCsv` and serialize object arrays with `toCsv`.
- Read fixed-width files with `fixedWidthFile` and `fixedWidthFileStream`.
- `loopDateRange` iterates over a date range with configurable step intervals.

### Statistics (`analyticsLibrary.Statistics`)

All methods are extension methods on `double[]` or `IEnumerable<double>`:

| Method | Description |
|---|---|
| `standardDeviation()` | Population standard deviation |
| `variance()` | Population variance |
| `covariance()` | Sample covariance matrix (rows = observations, cols = variables) |
| `normalize(min, max)` | Maps values to `[min, max]`, default `[0, 1]` |
| `histogram(steps)` | Returns bucket counts for the given number of bins |
| `dot(other)` | Dot product of two `double[]` vectors |
| `norm()` | Euclidean norm (L2) of a vector |

### Algorithms (`analyticsLibrary.Algorithms`)

- `mergeSort<T>()` — stable recursive merge sort; supports ascending and descending order for `int`, `double`, `float`, `decimal`, and `string`.
- `quickSort<T>()` — in-place quick sort; same type support as merge sort.
- `sorting.pickFunction<T>()` — returns the comparison delegate for a given type; useful for supplying a custom comparator.

### Core Helpers (`analyticsLibrary.Core`)

- `dataTypeHelper.dataTypeFromString(string)` — maps SQL/ODBC type name strings (e.g. `"varchar"`, `"int"`) to `dataTypeEnum` values.
- `analytics` — `compress()`, `titleCase()`, `wordReplace()` string extension methods.
- `extensions.fromSasEpochDate()` — parses a SAS datetime string in `ddMMMyyyy:hh:mm:ss.fff` format.

### Excel (`analyticsLibrary.Excel`)

- Read and write Excel workbooks and individual worksheets as `DataSet`/`DataTable`.
- Target individual cells in an existing workbook.

### Access (`analyticsLibrary.Access`, Windows)

- Open and query Access `.mdb`/`.accdb` database files via OleDb.

### Hadoop (`analyticsLibrary.Hadoop`)

- Read data from Hadoop clusters via an ODBC connection string.

---

## Repository Layout

```
.github/
  workflows/
    ci.yml        # PR: build, test, pack, upload beta artifacts
    release.yml   # Tag/dispatch: build, test, pack, deploy to NuGet
src/
  analyticsLibrary.Core/
  analyticsLibrary.Algorithms/
  analyticsLibrary.Statistics/
  analyticsLibrary.Excel/
  analyticsLibrary.Access/
  analyticsLibrary.Hadoop/
  analyticsLibrary/           # transition metapackage
  analyticsLibraryInstaller/
  release files/
tests/
  analyticsLibrary.Core.Tests/
  analyticsLibrary.Statistics.Tests/
  analyticsLibrary.Algorithms.Tests/
docs/
  epics/
    epic-001-remodel.plan.md
Directory.Packages.props      # centralized NuGet version management
Analytics Library.sln
```

---

## Build and Test

Requirements: .NET 8 SDK or later.

```shell
# Restore dependencies
dotnet restore

# Build all projects (Release)
dotnet build -c Release

# Run all unit tests
dotnet test -c Release

# Pack NuGet packages into ./nupkgs
dotnet pack -c Release -o ./nupkgs
```

---

## Release Process

**Beta packages (pull requests)**

Opening or updating a pull request against `main` triggers `ci.yml`. The workflow builds, tests, packs beta packages versioned `2.0.0-beta.pr-<pr>.<run>`, and uploads them as workflow artifacts. Beta packages are only pushed to NuGet.org when the `NUGET_API_KEY` secret is available and the PR is not from a fork.

**Release packages**

Push a version tag (e.g. `v2.0.0`) or trigger `release.yml` manually via workflow dispatch. The workflow:
1. Builds and tests the solution from the tagged commit.
2. Packs all seven packages.
3. Verifies all expected `.nupkg` files are present.
4. Deploys to NuGet.org under the `nuget-release` GitHub Environment, which requires explicit approval before the deploy step runs.

Store the NuGet API key as a repository secret named `NUGET_API_KEY`. Never commit credentials.

---

## Contributing

- Keep packages focused. Core must not depend on Excel, Access, Hadoop, or provider-specific packages.
- Add tests for all new public logic before submitting a PR. Tests live in `tests/`.
- `analyticsLibrary.Excel` uses NPOI (Apache-2.0) and ExcelDataReader (MIT)—no commercial license required. All Excel read/write operations are FOSS.
- `System.Data.OleDb` is Windows-only at runtime. Keep it isolated to `analyticsLibrary.Access`.
- Package versions are centrally managed in `Directory.Packages.props`. Add new dependencies there first.

---

## Migration Guide

### From `analyticsLibrary` 1.x to 2.0.0

**Removed packages and namespaces**

| Removed namespace | Removed package reference | Reason |
|---|---|---|
| `analyticsLibrary.dbObjects.sqlDb` | `System.Data.SqlClient`, `EntityFramework`, `SimpleImpersonation` | SQL Server support removed |
| `analyticsLibrary.oracle` | `Oracle.ManagedDataAccess.Core` | Oracle support removed |
| `analyticsLibrary.sas` | `OO_DBMS.ADODBCoreWrapper` | SAS support removed |
| `analyticsLibrary.sybase` | — | Sybase support removed |

**Steps to migrate**

1. Remove `analyticsLibrary` 1.x from your project.
2. Add the focused 2.x packages you need (see [Installation](#installation)).
3. Update `using` directives to the new namespaces:
   - `analyticsLibrary.library` → `analyticsLibrary.Core`
   - `analyticsLibrary.cs` → `analyticsLibrary.Algorithms`
   - `analyticsLibrary.statistics` → `analyticsLibrary.Statistics`
   - `analyticsLibrary.excelLibrary` → `analyticsLibrary.Excel`
   - `analyticsLibrary.access` → `analyticsLibrary.Access`
   - `analyticsLibrary.hadoop` → `analyticsLibrary.Hadoop`
4. Remove any references to the deleted namespaces (`sqlDb`, `oracle`, `sas`, `sybase`) and their package references listed above.
5. If you called `fromSasDate()`, rename it to `fromSasEpochDate()`.

**Target framework**: Projects must target `net8.0` or later. `netcoreapp3.1` is no longer supported.

### From `analyticsLibrary.Excel` 2.x to 3.0.0

`analyticsLibrary.Excel` 3.0.0 removes EPPlus and OleDb entirely. All Excel read and write operations now use **NPOI** (Apache-2.0) and **ExcelDataReader** (MIT).

**Removed public APIs**

| Removed member | Replacement |
|---|---|
| `getSheet(string, string)` / `getSheet(ExcelWorkbook, string)` | Use `new XSSFWorkbook(stream)` / `workbook.GetSheet(name)` from NPOI directly |
| `writeSheetDataXlsx(…, Action<ExcelWorksheet>)` overloads | Use `writeSheetDataXlsx(fileName, sheetName, object[,])` or the `IEnumerable<T>` overload |
| `writeWorkbook(…, Action<ExcelWorksheet>)` overload | Use `writeWorkbook(fileName, dataTable)` then open with NPOI to apply formatting |
| `numberFormatRow(ExcelWorksheet, int, string)` | Apply cell styles directly via NPOI `ISheet` / `IRow` |
| `numberFormatColumn(ExcelWorksheet, int, string)` | Apply cell styles directly via NPOI `ISheet` / `IColumn` |
| `getExcelConnection` | Removed; no replacement (EPPlus package removed) |

**New / expanded APIs (3.0.0)**

- `getWorkbookSheetDatasets(string fileName)` — returns `DataSet[]`, one per worksheet; `.xlsx` sheets include per-table `DataTable` instances for Excel structured tables; all three formats supported.
- `getWorkbookSheetDatasets(Stream stream, string formatHint)` — stream overload for in-memory use.
- `writeSheetDataXlsx(string, string, object[,], bool)` — write data grid to .xlsx, optionally deleting existing file.
- `writeSheetDataXlsx<T>(string, string, IEnumerable<T>)` — write typed list with auto header row.
- `copySheet(string, string, string)` — clone a worksheet within an .xlsx file.
- `deleteTable(string, string, string)` — remove a named Excel structured table from a sheet.

**xlsb write**: Writing `.xlsb` files is not supported. Calls to write APIs with a `.xlsb` path throw `NotSupportedException`. Reading `.xlsb` is supported via ExcelDataReader.
