# analyticsLibrary <!-- omit in toc -->
analyticsLibrary is a project that's been built up over time.  In my work analyzing, cleaning, and presenting large data sets, the operations in this library have proven useful. The plan is clean-up this port from my .Net Framework version to a .Net Core friendly format.

There are a lot of different kinds of tools here.  I plan to add to it, re-organize the different areas of functionality, and clean up the parts that didn't port over from .Net Framework very well.  Thank you for your patience.  Please, log any issues or put in Pull Requests to contribute to the code base.

## Index <!-- omit in toc -->
- [Available Operations](#available-operations)
  - [CSV](#csv)
  - [Computer Science Operations](#computer-science-operations)
  - [MS Excel File](#ms-excel-file)
  - [DataBase Server Access](#database-server-access)
  - [Statistics](#statistics)
- [Related Links](#related-links)

## Available Operations

### CSV
   - Reads and writes CSV file format with customizable field delimiter
   - Can handle variable or fixed with columns

### Computer Science Operations
   1. Searching
      - Strongly typed value search.  In the process of releasing a binary search algorithm.
   2. Sorting
      - Merge
      - Quick

### MS Excel File
   - Read/Write Excel File into a DataSet
   - Read/Write Excel Sheet in to a DataTable
   - Can target cells in an existing file

### DataBase Server Access
   - MS Access File
     - Allows for programmatic access and querying of Access dbs.  I need to work through the API and see how well it works after the port.
   - Hadoop Server
   - Oracle Server
   - SAS Server
   - MS SQL Server - This part of the code base didn't port over well.  It needs some overhaul and redesign to fit the .Net Core paradigm.
   - Sybase

### Statistics
   - Co-variance
   - Dot Product
   - Histogram
   - Normalize
   - Standard Deviation
   - Variance

## Related Links
Nuget package: [analyticsLibrary](https://www.nuget.org/packages/analyticsLibrary/)