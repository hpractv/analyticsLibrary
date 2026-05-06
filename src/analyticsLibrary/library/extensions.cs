using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static analyticsLibrary.library.loopDateRange;

namespace analyticsLibrary.library
{
    public static class extensions
    {
        public static string stringOrNull(this object value, int length)
        {
            var returnObject = value.stringOrNull();
            if (!string.IsNullOrWhiteSpace(returnObject) && returnObject.Length > length)
            {
                returnObject = returnObject.Substring(0, length);
            }
            return returnObject;
        }

        public static string stringOrNull(this object value)
        {
            var returnObject = (string)null;
            if (value != null && value.ToString().Trim().Length > 0)
            {
                returnObject = value.ToString().Trim();
            }
            return returnObject;
        }

        public static DateTime? dateOrNull(this object value)
        {
            var returnObject = value.stringOrNull();
            if (returnObject != null)
                return returnObject.toDateTime();

            return null;
        }

        public static int? intOrNull(this object value)
            => value.stringOrNull() == null ? (int?)null : int.Parse(value.ToString());

        public static decimal? decimalOrNull(this object value)
            => value.stringOrNull() == null ? (decimal?)null : decimal.Parse(value.ToString());

        public static double? doubleOrNull(this object value)
            => value.stringOrNull() == null ? (double?)null : double.Parse(value.ToString());

        public static float? floatOrNull(this object value)
            => value.stringOrNull() == null ? (float?)null : float.Parse(value.ToString());

        public static bool greaterThan(this DateTime value, string stringValue)
        {
            return value > DateTime.Parse(stringValue);
        }

        public static bool greaterThanEqual(this DateTime value, string stringValue)
        {
            return value >= DateTime.Parse(stringValue);
        }

        public static bool lessThan(this DateTime value, string stringValue)
        {
            return value < DateTime.Parse(stringValue);
        }

        public static bool lessThanEqual(this DateTime value, string stringValue)
        {
            return value <= DateTime.Parse(stringValue);
        }

        public static bool between(this DateTime value, string fromDate, string toDate)
        {
            return value.greaterThanEqual(fromDate) && value.lessThanEqual(toDate);
        }

        public static bool between(this DateTime value, DateTime fromDate, DateTime toDate)
        {
            return value >= (fromDate) && value <= toDate;
        }

        public static DateTime? fromSasDate(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (DateTime?)null;
            else
                return DateTime.ParseExact(value, @"ddMMMyyyy:hh:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        public static DateTime toDateTime(this string value)
        {
            return DateTime.Parse(value);
        }

        public static void forEach<t>(this IEnumerable<t> values, Action<t> forEachMethod)
        {
            values.ToList().ForEach(value => forEachMethod(value));
        }

        public static int binarySearch<t>(this t[] values, t value)
        {
            return Array.BinarySearch(values, value);
        }

        public static indexObject<v>[] index<v>(this IEnumerable<v> values)
        //=> values.index<int, v>(i => i).Cast<indexObject<v>>().ToArray();
        {
            var index = 0;

            return values
                .Select(value => new indexObject<v>(index++, value))
                .ToArray();
        }


        public static indexObject<t, v>[] index<t, v>(this IEnumerable<v> values, Func<int, t> indexMethod)
        {
            var index = 0;

            return values
                .Select(value => new indexObject<t, v>(indexMethod, index++, value))
                .ToArray();
        }

        public static string columns(this DataRow row)
        {
            return row.columns(false);
        }

        public static string columns(this DataRow row, bool csvQuotes)
        {
            return row.Table.columns(csvQuotes);
        }

        public static string columns(this DataTable table)
        {
            return table.columns(false);
        }

        public static string columns(this DataTable table, bool csvQuotes)
        {
            return string.Join(",",
                table.Columns.Cast<DataColumn>().Select(c => csvQuotes ? "\"" + c.ColumnName + "\"" : c.ColumnName).ToArray());
        }

        public static string tables(this DataSet set)
        {
            return set.tables(false);
        }

        public static string tables(this DataSet set, bool csvQuotes)
        {
            return string.Join(",",
                set.Tables.Cast<DataTable>().Select(c => csvQuotes ? "\"" + c.TableName + "\"" : c.TableName).ToArray());
        }

        private static void hmsFromS(decimal time, out decimal hours, out decimal minutes, out decimal seconds)
        {
            hours = Math.Floor(time / 3600);
            minutes = Math.Floor(time / 60) - (hours * 60);
            seconds = (time - (hours * 3600) - (minutes * 60));
        }

        public static List<v> loop<t, v>(this IEnumerable<t> values, Func<t, IEnumerable<v>> loopMethod) => values.loop(1, loopMethod, null, false, true);

        public static List<v> loop<t, v>(this IEnumerable<t> values, Func<t, IEnumerable<v>> loopMethod, bool displayStep) => values.loop(1, loopMethod, null, displayStep, true);

        public static List<v> loop<t, v>(this IEnumerable<t> values, int step, Func<IEnumerable<t>, IEnumerable<v>> loopMethod) => loop<t, v>(values, step, loopMethod, false);

        public static List<v> loop<t, v>(this IEnumerable<t> values, int step, Func<IEnumerable<t>, IEnumerable<v>> loopMethod, bool displayStep) => values.loop(step, null, loopMethod, displayStep, false);

        private static List<v> loop<t, v>(this IEnumerable<t> values, int step, Func<t, IEnumerable<v>> loopSingleMethod, Func<IEnumerable<t>, IEnumerable<v>> loopMultipleMethod, bool displayStep, bool single)
        {
            var start = DateTime.Now;
            var returnValues = new List<v>();
            var counter = 1;
            var count = values.Count();
            var ofCount = Math.Ceiling((double)count/(double)step);
            var steps = ofCount.ToString("###,###,###,###");

            for (var i = 0; i < count; i += step, counter++)
            {
                var subValues = values.Skip(i).Take(step);

                if (single)
                    returnValues.AddRange(loopSingleMethod(subValues.First()));
                else
                    returnValues.AddRange(loopMultipleMethod(subValues));

                if (displayStep)
                {
                    var totalTime = DateTime.Now.Subtract(start).TotalSeconds;
                    var timeElapsed = i == 0 ? "N/A" : (totalTime  / 60.0).ToString("###,###,###.00 min(s)");
                    var timeLeft = i == 0 ? "N/A" : (((totalTime / i) * (count - i)) / 60.0).ToString("###,###,###.00 min(s)");
                    Console.WriteLine("____________________");
                    Console.WriteLine($"Step: {counter}/{steps}" + Environment.NewLine +
                        $"Elapsed Time: {timeElapsed}" + Environment.NewLine +
                        $"Est. Time Left: {timeLeft}");
                }
            }

            if (displayStep)
            {
                var totalTime = (decimal)DateTime.Now.Subtract(start).TotalSeconds;
                hmsFromS(totalTime, out var hours, out var minutes, out var seconds);
                hmsFromS(totalTime / (decimal)ofCount, out var avgHours, out var avgMinutes, out var avgSeconds);

                Console.WriteLine("____________________");
                Console.WriteLine("Total Steps: {0}" + Environment.NewLine +
                    "Total Time: {1:00}:{2:00}:{3:00.00}" + Environment.NewLine +
                    "Avg. Step Time: {4:00}:{5:00}:{6:00.00}" + Environment.NewLine,
                    steps, hours, minutes, seconds, avgHours, avgMinutes, avgSeconds);
            }
            return returnValues;
        }

        public static List<v> loop<v>(this loopDateRange dates, Func<DateTime, DateTime, bool, IEnumerable<v>> loopMethod)
        {
            var loopStart = dates.start;
            var loopEnd = DateTime.Now;
            var output = new List<v>();
            var firstPass = true;
            while (dates.start < dates.end)
            {
                switch (dates.interval)
                {
                    case intervalEnum.hour:
                        loopEnd = loopStart.AddHours(1);
                        break;

                    case intervalEnum.day:
                        loopEnd = loopStart.AddDays(1);
                        break;

                    case intervalEnum.year:
                        loopEnd = loopStart.AddYears(1);
                        break;

                    case intervalEnum.month:
                    default:
                        loopEnd = loopStart.AddMonths(1);
                        break;
                }
                if (loopEnd > dates.end) loopEnd = dates.end;

                output.AddRange(loopMethod(loopStart, loopEnd, firstPass));

                firstPass = false;
                loopStart = loopEnd;
            }
            return output;
        }

        public static void loop(this loopDateRange dates, Action<DateTime, DateTime, bool> loopMethod)
        {
            var loopStart = dates.start;
            var loopEnd = DateTime.Now;
            var firstPass = true;
            while (loopStart < dates.end)
            {
                switch (dates.interval)
                {
                    case intervalEnum.hour:
                        loopEnd = loopStart.AddHours(1);
                        break;

                    case intervalEnum.day:
                        loopEnd = loopStart.AddDays(1);
                        break;

                    case intervalEnum.year:
                        loopEnd = loopStart.AddYears(1);
                        break;

                    case intervalEnum.month:
                    default:
                        loopEnd = loopStart.AddMonths(1);
                        break;
                }
                loopEnd = loopEnd.AddSeconds(-1);

                if (loopEnd > dates.end) loopEnd = dates.end;
                if (loopStart < loopEnd) loopMethod(loopStart, loopEnd, firstPass);

                firstPass = false;
                loopStart = loopEnd.AddSeconds(1);
            }
        }

        public static void batch<t>(this IEnumerable<t> values, int size, Action<IEnumerable<t>> process)
            => values.batch(size, process, false);

        public static void batch<t>(this IEnumerable<t> values, int size, Action<IEnumerable<t>> process, bool showProgress)
        {
            var batchCount = (int)Math.Ceiling((double)values.Count() / size);
            var start = DateTime.Now;
            var lastTime = DateTime.Now;
            var now = DateTime.Now;
            var total = 0.0;

            Parallel.For(0, batchCount, i =>
            {
                process(values.Skip(i * size).Take(size));
                if (showProgress)
                {
                    now = DateTime.Now;
                    var minutes = now.Subtract(lastTime).TotalMinutes;
                    total = now.Subtract(start).TotalMinutes;
                    var stepTime = total/i;

                    Console.WriteLine("____________________");
                    Console.WriteLine($"Step {i + 1}/{batchCount}: {minutes.ToString("00.00")} min");
                    Console.WriteLine($"Est. Left: { ((stepTime * batchCount) - total).ToString("00.00")} min(s)");
                    lastTime = now;
                }
            });
            var end = DateTime.Now;
            if (showProgress)
            {
                now = DateTime.Now;
                total = now.Subtract(start).TotalMinutes;
                Console.WriteLine("____________________");
                Console.WriteLine($"Total time: { total.ToString("00.00")} min(s))");
                lastTime = now;
            }
        }

        public static string stringJoin<t>(this IEnumerable<t> values, char seperator)
        {
            return values.stringJoin(seperator.ToString());
        }

        public static string stringJoin<t>(this IEnumerable<t> values, string seperator)
        {
            return string.Join(seperator, values);
        }

        public static int indexOf<t>(this IEnumerable<t> values, t value)
        {
            var index = -1;
            if (values != null & values.Count() > 0)
            {
                var valuesArray = values.ToArray();
                for (var i = 0; i < valuesArray.Length; i++)
                {
                    if (valuesArray[i].Equals(value))
                    {
                        index = i;
                        break;
                    }
                }
            }
            if (index == -1) throw new IndexOutOfRangeException("Value not part of collection.");
            return index;
        }

        public static string selectClass(this IEnumerable<DataRow> rows)
        {
            return rows.First().Table.selectClass(null);
        }

        public static string selectClass(this IEnumerable<DataRow> rows, string className)
        {
            return rows.First().Table.selectClass(className);
        }

        public static string selectClass(this DataTable table)
        {
            var columns = table.Columns.Cast<DataColumn>()
            .Select(dc => new { dc.ColumnName, DataType = dc.DataType.ToString().Replace("System.", string.Empty).ToLower() });
            return selectClass(columns.Select(c => c.ColumnName), columns.Select(c => c.DataType.ToString()), null, "r");
        }

        public static string selectClass(this DataTable table, string className)
        {
            var columns = table.Columns.Cast<DataColumn>()
            .Select(dc => new { dc.ColumnName, DataType = dc.DataType.ToString().Replace("System.", string.Empty).ToLower() });
            return selectClass(columns.Select(c => c.ColumnName), columns.Select(c => c.DataType.ToString()), className, "r");
        }

        public static string selectClass(this IEnumerable<string> values, IEnumerable<string> types, string className, string alias)
        {
            var typesA = types
            .Select(t => t == "datetime" ? "DateTime" :
                t == "int32" ? "int" :
                t)
            .ToArray();

            return ".Rows.Cast<DataRow>()" + Environment.NewLine +
                string.Format(".Select({0} => new {1}{{" + Environment.NewLine, alias,
                    string.Format("{0}", !string.IsNullOrWhiteSpace(className) ? className + "()" : string.Empty)) +
                values
                    .Select(h =>
                        typesA[values.indexOf(h)] == "string" ?
                            string.Format("    {0} = {1}[\"{2}\"].stringOrNull(),", h.fieldName(), alias, h) :
                        typesA[values.indexOf(h)] == "int" ?
                            string.Format("    {0} = {1}[\"{2}\"].intOrNull(),", h.fieldName(), alias, h) :
                        typesA[values.indexOf(h)] == "double" ?
                            string.Format("    {0} = {1}[\"{2}\"].doubleOrNull(),", h.fieldName(), alias, h) :
                        typesA[values.indexOf(h)] == "decimal" ?
                            string.Format("    {0} = {1}[\"{2}\"].decimalOrNull(),", h.fieldName(), alias, h) :
                        typesA[values.indexOf(h)] == "float" ?
                            string.Format("    {0} = {1}[\"{2}\"].floatOrNull(),", h.fieldName(), alias, h) :
                        typesA[values.indexOf(h)] == "DateTime" ?
                            string.Format("    {0} = {1}[\"{2}\"].dateOrNull(),", h.fieldName(), alias, h) :
                        //else
                            string.Format("    {0} = ({1}){2}[\"{3}\"],", h.fieldName(), typesA[values.indexOf(h)], alias, h))
                    .stringJoin(Environment.NewLine) + Environment.NewLine + "})";
        }

        public static string buildClass(this IEnumerable<DataRow> rows, string className) => rows.First().Table.buildClass(className);
        
        public static string buildClass(this DataTable table, string className)
        {
            var columns = table.Columns.Cast<DataColumn>()
            .Select(dc => new { dc.ColumnName, DataType = dc.DataType.ToString().Replace("System.", string.Empty).ToLower() });

            return buildClass(columns.Select(c => c.ColumnName), columns.Select(c => c.DataType), className);
        }

        public static string buildClass<t>(this IEnumerable<t> values, string className) => values.buildClass(null, className);

        public static string buildClass<t>(this IEnumerable<t> values, IEnumerable<string> types, string className)
        {
            var typesA = types == null ? (string[])null : types.ToArray();
            return (@"public class " + className + @" {" + Environment.NewLine +
                values
                    .Select(h => string.Format(
                        "    public {0} {1} {{ get; set; }}",
                        (types == null ?
                            "object" :
                         typesA[values.indexOf(h)] == "datetime" ?
                            "DateTime" :
                        typesA[values.indexOf(h)] == "int16" || typesA[values.indexOf(h)] == "int32"  ?
                            "int" :
                         //else
                         typesA[values.indexOf(h)]),
                         h.fieldName()))
                    .stringJoin(Environment.NewLine) +
            @"
}");
        }

        public static string fieldName(this object value)
        {
            var names = value
            .ToString()
                .Replace(@"-", " ")
                .Replace(@"/", " ")
                .Replace(@"\", " ")
                .Replace(@"(", " ")
                .Replace(@")", " ")
            .Split()
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

            return names
                .Select(v => names.indexOf(v) == 0 ?
                   v.ToLower() :
                   v.Substring(0, 1).ToUpper() +
                       (v.Length > 1 ? v.Substring(1, v.Length - 1).ToLower() : string.Empty))
                .stringJoin("");
        }

        public static string[] splitFromPositions(this string line, int[] positions)
        {
            var returnValues = new List<string>();
            for (var i = 0; i < positions.Length; i++)
            {
                var value = (string)null;
                var lastPos = i == 0 ? 0 : positions[i - 1];
                var curPos = positions[i];
                //string.Format("{0}: {1}-{2}", i, lastPos, curPos).Dump();

                if (curPos <= line.Length)
                {
                    value = line.Substring(lastPos, curPos - lastPos);
                }
                returnValues.Add(value.stringOrNull());
            }
            if (positions.Last() < line.Length)
            {
                returnValues.Add(line.Substring(positions.Last()).stringOrNull());
            }

            //"------".Dump();
            return returnValues.ToArray();
        }

        public static string[] splitFromLengths(this string line, int[] lengths)
        {
            var returnValues = new List<string>();
            var runningLength = 0;
            for (var i = 0; i < lengths.Length; i++)
            {
                var length = lengths[i];
                returnValues.Add(line.Substring(runningLength, length).stringOrNull());
                runningLength += length;
            }
            if (runningLength < line.Length)
                returnValues.Add(line.Substring(runningLength).stringOrNull());

            return returnValues.ToArray();
        }

        public static DateTime startOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public const string countFormat = "###,###,###,###,###,##0";
        public const string percentFormat = "###0.00%";
        public const string decimalFormat = "###,###,###,##0.00";
        public const string moneyFormat = "$###,###,###,##0.00";

        public static string toCountString<t>(this IEnumerable<t> values)
        {
            return values.Count().toCountString();
        }

        public static string toCountString<t>(this IEnumerable<t> values, Predicate<t> filter)
        {
            return values.Count(v => filter(v)).toCountString();
        }

        public static string toCountString(this int value)
        {
            var formatted = "0";
            if (value != 0)
            {
                formatted = value.ToString(countFormat);
            }
            return formatted;
        }

        public static string toPercentString(this double value)
        {
            return ((decimal)value).toPercentString();
        }

        public static string toPercentString(this decimal value)
        {
            var formatted = "0.00%";
            if (value != 0)
            {
                formatted = value.ToString(percentFormat);
            }
            return formatted;
        }

        public static string toDecimalString(this double value)
        {
            return ((decimal)value).toDecimalString();
        }

        public static string toDecimalString(this decimal value)
        {
            var formatted = "0.00";
            if (value != 0)
            {
                formatted = value.ToString(decimalFormat);
            }
            return formatted;
        }

        public static string toMoneyString(this double value)
        {
            return ((decimal)value).toMoneyString();
        }

        public static string toMoneyString(this decimal value)
        {
            var formatted = "$0.00";
            if (value != 0)
            {
                formatted = value.ToString(moneyFormat);
            }
            return formatted;
        }

        public static string toPaddedShortDateString(this DateTime value)
        {
            return value.toPaddedShortDateString(true);
        }

        public static string toPaddedShortDateString(this DateTime value, bool fourDigitYear)
        {
            return string.Format("{0}/{1}/{2}",
                value.Month.ToString().PadLeft(2, '0'),
                value.Day.ToString().PadLeft(2, '0'),
                fourDigitYear ? value.Year.ToString() : value.Year.ToString().Substring(2, 2));
        }

        public static bool hasAny<T>(this IEnumerable<T> values, IEnumerable<T> contains)
        {
            bool hasAny = false;
            foreach (T containsValue in contains)
            {
                if (hasValue(values, containsValue))
                {
                    hasAny = true;
                    break;
                }
            }
            return hasAny;
        }

        public static bool hasAll<T>(this IEnumerable<T> values, IEnumerable<T> contains)
        {
            bool hasAll = true;
            foreach (T containsValue in contains)
            {
                if (!hasValue(values, containsValue))
                {
                    hasAll = false;
                    break;
                }
            }
            return hasAll;
        }

        public static bool hasValue<T>(this IEnumerable<T> values, T value)
        {
            bool hasValue = false;

            if (value == null)
            {
                if (values.Any(v => v == null)) hasValue = true;
            }
            else
            {
                foreach (T item in values)
                {
                    if (item is string && item.ToString().ToLower() == value.ToString().ToLower())
                    {
                        hasValue = true;
                        break;
                    }
                    else if (item != null && item.Equals(value))
                    {
                        hasValue = true;
                        break;
                    }
                }
            }
            return hasValue;
        }

        public static bool trueValue(this bool? value)
        {
            var returnValue = false;
            if (value.HasValue && value.Value)
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static IEnumerable<t> appendSet<t>(this IEnumerable<t> a, IEnumerable<t> b)
        {
            var returnSet = new t[a.Count() + b.Count()];
            a.ToArray().CopyTo(returnSet, 0);
            b.ToArray().CopyTo(returnSet, a.Count());
            return returnSet;
        }

        public static string toFileShortDateString(this DateTime value)
        {
            return value.toFileShortDateString(false);
        }

        public static string toFileShortDateString(this DateTime value, bool includeTime)
        {
            var date = string.Format("{0}{1}{2}",
                value.Year.ToString(),
                value.Month.ToString().PadLeft(2, '0'),
                value.Day.ToString().PadLeft(2, '0'));

            var time = !includeTime ? string.Empty :
                string.Format("{0}{1}{2}",
                    value.Hour.ToString("00"),
                    value.Minute.ToString("00"),
                    value.Second.ToString("00"));

            return string.Format("{0}{1}", date, time);
        }

        public static decimal perValue<t>(this IEnumerable<t> values, Predicate<t> sumFilter, Func<t, decimal> value)
        {
            return values.perValue(sumFilter, sumFilter, value);
        }

        public static decimal perValue<t>(this IEnumerable<t> values, Predicate<t> sumFilter, Predicate<t> perFilter, Func<t, decimal> value)
        {
            if (values.Count(v => perFilter(v)) == 0)
                return 0;
            else
                return (decimal)values.Where(v => sumFilter(v)).Sum(v => value(v)) /
                    (decimal)values.Count(v => perFilter(v));
        }

        public static decimal percentage<t>(this IEnumerable<t> values, Predicate<t> filter)
        {
            return (decimal)values.Count(v => filter(v)) / (decimal)values.Count();
        }

        public static string percentageFormatted<t>(this IEnumerable<t> values, Predicate<t> filter)
        {
            return values.percentage(filter).toPercentString();
        }

        public static string lineValueByPosition(this string value, int fromPosition, int toPosition, bool trim)
        {
            return value.lineValue(fromPosition, toPosition - fromPosition, trim);
        }

        public static string lineValue(this string value, int fromPosition, int length, bool trim)
        {
            string returnValue = null;
            if (value != null)
            {
                if (value.Length > length - fromPosition)
                {
                    returnValue = value.Substring(fromPosition, length);
                }
                else
                {
                    returnValue = value.Substring(fromPosition, value.Length - fromPosition);
                }
            }

            if (trim)
                returnValue = returnValue.Trim();

            return returnValue;
        }

        public static string toLength(this string value, int length)
        {
            var returnValue = (string)null;
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
            {
                if (value.Length < length)
                    returnValue = value;
                else
                    returnValue = value.Substring(0, length);
            }

            return returnValue;
        }

        public static int stringIndex(this string[] strings, string value)
        {
            var i = 0;
            var found = false;
            var lowerValue = value.ToLower();
            var lowerStrings = strings.Select(s => s.ToLower()).ToArray();

            for (i = 0; i < lowerStrings.Length; i++)
            {
                if (lowerStrings[i].ToString().Equals(lowerValue))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new ApplicationException("String value not found.");

            return i;
        }

        public static object valueOf(this string[] columns, object[] values, string value)
        {
            return values[columns.stringIndex(value)];
        }

        public static v valueOrNull<t, v>(this Dictionary<t, v> values, t key)
        {
            return (v)(values.ContainsKey(key) ? values[key] : (object)null);
        }

        public static IEnumerable<v> valueOrNull<t, v>(this Lookup<t, v> values, t key)
        {
            return (values.Contains(key) ? values[key] : (IEnumerable<v>)null);
        }

        #region day calculations

        public static DateTime firstOfMonth(this DateTime value)
        {
            return DateTime.Parse(string.Format("{0}/1/{1}", value.Month.ToString(), value.Year.ToString()));
        }

        public static DateTime endOfMonth(this DateTime value)
        {
            return value.firstOfMonth().AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// This function returns the date of a previous day of the week to the specified date.
        /// e.g.: "Last Tuesday"
        /// </summary>
        /// <param name="beforeDate">Reference date to search before</param>
        /// <param name="day">Day of the week to retrieve date for</param>
        /// <returns>Date of the last day of the week prior to a specific date</returns>
        public static DateTime lastDayDate(this DateTime beforeDate, DayOfWeek day)
        {
            var lastDay = beforeDate.Date;
            for (var i = 0; i < 7; i++)
            {
                lastDay = lastDay.AddDays(-1);
                if (lastDay.DayOfWeek == day && lastDay < beforeDate)
                    break;
            }
            return lastDay;
        }

        public static int quarter(this DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                case 2:
                case 3:
                    return 1;

                case 4:
                case 5:
                case 6:
                    return 2;

                case 7:
                case 8:
                case 9:
                    return 3;

                case 10:
                case 11:
                case 12:
                    return 4;

                default:
                    throw new ApplicationException("Invalid month for calculating quarter.");
            }
        }

        public static int numberOfMonths(this DateTime end, DateTime start)
            => ((end.Year - start.Year) * 12) + (end.Month - start.Month);
        
        #endregion day calculations

        public static bool notNullHasValue<t>(this IEnumerable<t> values)
        {
            return values != null && values.Count() > 0;
        }

        public static string description(this Enum value)
        {
            var attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static t enumValueOf<t>(this string value)
        {
            var enumValues = (t[])Enum.GetValues(typeof(t));
            foreach (var enumValue in enumValues)
            {
                var description = ((Enum)Enum.Parse(typeof(t), enumValue.ToString()))
                    .description()
                    .ToLower();

                if (value.ToLower().Equals(description))
                {
                    return enumValue;
                }
            }

            throw new ArgumentException("The string is not a description or value of the specified enum.");
        }
    }
}