using System;
using System.Data;
using System.Linq;
using analyticsLibrary.Core;
namespace analyticsLibrary.Excel
{
    public static class Extensions
    {
        public static int columnNumber(this columnLettersEnum value)
        {
            return (int)value;
        }

        public static void autoSizeColumns(this object workbook)
        {
            foreach (var sheet in ((dynamic)workbook).Worksheets)
            {
                sheet.autoSizeColumns();
            }
        }
        public static void numberFormatRow(this object sheet, int row, string format)
        {
            var formatRow = ((dynamic)sheet).Row(row);
            formatRow.Style.Numberformat.Format = format;
        }
        public static void numberFormatRows(this object sheet, int[] rows, string format)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                numberFormatRow((dynamic)sheet, rows[i], format);
            }
        }

        public static void numberFormatColumn(this object sheet, int column, string format)
        {
            var formatColumn = ((dynamic)sheet).Column(column);
            formatColumn.Style.Numberformat.Format = format;
        }
        public static void numberFormatColumns(this object sheet, int[] columns, string format)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                numberFormatColumn((dynamic)sheet, columns[i], format);
            }
        }

        public static object rowValue<TMember>(this DataRow row, string field)
        {
            return row.rowValue<TMember, object>(field);
        }
        public static TField rowValue<TMember, TField>(this DataRow row, string field)
        {
            var fieldName = string.Empty;
            var attributes = typeof(TMember).GetMember(field)[0]
                .GetCustomAttributes(true);
            var attribute = attributes.FirstOrDefault(f => f is sheetColumnAttribute) as sheetColumnAttribute;

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
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ColumnNotFoundException();
            if (!row.containsColumn(fieldName)) throw new ColumnNotFoundException();

            return (TField)(row.IsNull(fieldName) ? null : row.Field<object>(fieldName));
        }
    }

    public class ColumnNotFoundException : ApplicationException { }
}
