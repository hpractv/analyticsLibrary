using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace analyticsLibrary.library
{
    public static class csv_extension
    {
        public static string[] fromCsv(this string values)
            => values.fromCsv(',');
        
        public static string[] fromCsv(this string values, params char[] delimiters)
        {
            var parsed = new List<string>();
            if (!values.Contains(@"""") || !delimiters.hasValue(','))
            {
                parsed.AddRange(values.Split(delimiters).Select(v => v.stringOrNull()));
            }
            else
            {
                var chars = values.ToCharArray();
                var length = values.Length;
                var startPos = 0;
                var inQuote = false;
                for (var i = 0; i < chars.Length; i++)
                {
                    var cChar = chars[i];
                    var pChar = i > 0 ? chars[i - 1] : ' ';
                    var lChar = i == chars.Length - 1;

                    if (cChar == '"' && (i == 0 || (i > 0 && pChar != '\\')))
                    {
                        inQuote = !inQuote;
                    }

                    if ((delimiters.hasValue(cChar) && !inQuote) || lChar)
                    {
                        var value = values.Substring(startPos, !lChar && !delimiters.hasValue(cChar) ? i - startPos + 1 : i - startPos);
                        if (value.StartsWith(@""""))
                        {
                            value = value.Substring(1, value.Length - 1);
                            if (value.EndsWith(@""""))
                            {
                                value = value.Substring(0, value.Length - 1);
                            }
                        }
                        value = value.Replace(@"""""", @"""");
                        parsed.Add(value.stringOrNull());
                        startPos = i + 1;
                    }

                    if (lChar && delimiters.hasValue(cChar))
                    {
                        parsed.Add(null);
                    }
                }
            }
            return parsed.ToArray();
        }

        public static string toCsv(this IEnumerable<object> values)
            => values.toCsv(',');
        
        public static string toCsv(this IEnumerable<object> values, char delimiter)
        {
            return values.Select(v =>
            {
                var returnValue = string.Empty;

                if (v != null)
                {
                    if (v is string)
                    {
                        var stringValue = v as string;
                        var needsQuotes = false;
                        if (stringValue.Contains('"'))
                        {
                            stringValue = stringValue.Replace(@"""", @"""""");
                            needsQuotes = true;
                        }
                        if (stringValue.Contains(delimiter))
                        {
                            needsQuotes = true;
                        }
                        if (needsQuotes)
                            returnValue = string.Format(@"""{0}""", stringValue);
                        else
                            returnValue = stringValue;
                    }
                    else
                    {
                        returnValue = v.ToString();
                    }
                }

                return returnValue;
            })
            .stringJoin(delimiter);
        }

        /// <summary>
        /// Converts a string array into an array of numeric types
        /// </summary>
        /// <typeparam name="t">Type to parse to</typeparam>
        /// <param name="values">Array of string values that can be cast to the desired type</param>
        /// <returns>Array of numeric parsed type</returns>
        /// <remarks>If the string array is parsed to int values, any portion beyond the decimal point will be truncated.</remarks>
        public static t[] toNumericArray<t>(this string[] values)
        {
            var type = typeof(t);
            var parse = type.GetMethod("Parse", new Type[] { typeof(string) });

            if (parse == null) throw new ApplicationException("Parse method not supported.");

            var ts = new t[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var parseValue = type.Name.ToLower().StartsWith("int") ?
                    values[i].Split('.')[0] : values[i];

                ts[i] = (t)parse.Invoke(parse, new object[] { parseValue });
            }

            return ts;
        }

        public static void writeCsv(this DataTable data, string outputFile)
           => data.writeCsv(outputFile, false);

        public static void writeCsv(this DataTable data, string outputFile, bool append)
            => data.writeCsv(outputFile, ',', append);

        public static void writeCsv(this DataTable data, string outputFile, char delimeter)
            => data.writeCsv(outputFile, delimeter, false);

        public static void writeCsv(this DataTable data, string outputFile, char delimeter, bool append)
            => data.Rows.Cast<DataRow>().Select(r => r.ItemArray)
                .writeCsv(outputFile, data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray(), delimeter, append);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile)
            => data.writeCsv(outputFile, false);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, bool append)
            => data.writeCsv(outputFile, ',', true, append);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, IEnumerable<string> header)
            => data.writeCsv(outputFile, header, false);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, IEnumerable<string> header, bool append)
            => data.writeCsv(outputFile, header, ',', append);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, IEnumerable<string> header, char delimiter)
            => data.writeCsv(outputFile, header, delimiter, false);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, IEnumerable<string> header, char delimiter, bool append)

        {
            var file = new List<object[]>();
            file.Add(header.Cast<object>().ToArray());
            file.AddRange(data);
            file.writeCsv(outputFile, delimiter, true, append);
        }

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, char delimeter, bool hasRowHeader)
            => data.writeCsv(outputFile, delimeter, hasRowHeader, false);

        public static void writeCsv(this IEnumerable<object[]> data, string outputFile, char delimeter, bool hasRowHeader, bool append)
        {
            var file = new StreamWriter(outputFile, append);
            if (!hasRowHeader)
            {
                file.WriteLine(data.First().index().Select(i => $"Column{i.index + 1}").toCsv(delimeter));
            }
            data.forEach(r => file.WriteLine(r.toCsv(delimeter)));
            file.Close();
        }
    }
}