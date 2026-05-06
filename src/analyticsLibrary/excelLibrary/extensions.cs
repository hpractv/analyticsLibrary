using System;
using System.Data;
using System.Linq;
using OfficeOpenXml;

namespace analyticsLibrary.excelLibrary
{
    public static class extensions
    {
        public static int columnNumber(this columnLettersEnum value)
        {
            return (int)value;
        }

        public static void autoSizeColumns(this ExcelWorkbook workbook)
        {
            foreach (var sheet in workbook.Worksheets)
            {
                sheet.autoSizeColumns();
            }
        }
        public static void autoSizeColumns(this ExcelWorksheet sheet)
        {
            var columnCount = sheet.Dimension.End.Column;
            for (var i = 1; i <= columnCount; i++)
            {
                sheet.Column(i).AutoFit();
            }
        }

        public static void numberFormatRow(this ExcelWorksheet sheet, int row, string format)
        {
            var formatRow = sheet.Row(row);
            formatRow.Style.Numberformat.Format = format;
        }
        public static void numberFormatRows(this ExcelWorksheet sheet, int[] rows, string format)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                sheet.numberFormatRow(rows[i], format);
            }
        }

        public static void numberFormatColumn(this ExcelWorksheet sheet, int column, string format)
        {
            var formatColumn = sheet.Column(column);
            formatColumn.Style.Numberformat.Format = format;
        }
        public static void numberFormatColumns(this ExcelWorksheet sheet, int[] columns, string format)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                sheet.numberFormatColumn(columns[i], format);
            }
        }

        public static object rowValue<type>(this DataRow row, string field)
        {
            return row.rowValue<type, object>(field);
        }
        public static valueType rowValue<type, valueType>(this DataRow row, string field)
        {
            var fieldName = string.Empty;
            var attributes = typeof(type).GetMember(field)[0]
                .GetCustomAttributes(true);
            var attribute = attributes.FirstOrDefault(f => f is sheetColumnAttrubte) as sheetColumnAttrubte;

            if (attribute != null)
            {
                foreach (var name in attribute.sheetColumnNames)
                {
                    if (row.containsColumn(name))
                    {
                        fieldName = name;
                        break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ApplicationException(string.Format("Field ({0}) does not exist in this table.", field));
            if (!row.containsColumn(fieldName)) throw new columnNotFoundException();

            return (valueType)(row.IsNull(fieldName) ? null : row.Field<object>(fieldName));
        }
        public class columnNotFoundException : ApplicationException { }
        public static bool containsColumn(this DataRow row, string column)
        {
            return row.Table.Columns.Cast<DataColumn>().Any(c => c.ColumnName.ToLower() == column.ToLower());
        }
    }
}
