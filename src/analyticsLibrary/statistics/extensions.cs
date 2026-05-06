using System;
using System.Linq;
using analyticsLibrary.library;
using System.Collections.Generic;

namespace analyticsLibrary.statistics
{
    public static class extensions
    {
        public static double standardDeviation(this int[] values)
        {
            return standardDeviation(values.Select(v => (double)v).ToArray());
        }

        public static double standardDeviation(this double[] values)
        {
            var mean = values.Average();
            return Math.Sqrt(values
                .Select(v => Math.Pow(v - mean, 2))
                .Sum() / (values.Length));
        }

        public static double variance(this int[] values)
        {
            return values.Select(v => (double)v).ToArray().variance();
        }

        public static double variance(this double[] values)
        {
            var mean = values.Average();
            return values
                .Select(v => Math.Pow(v - mean, 2))
                .Sum() / values.Length;
        }

        public static double[][] covariance(this int[][] values)
        {
            var dValues = new double[values.Length][];
            for (var i = 0; i < values.Length; i++) dValues[i] = values[i].Select(v => (double)v).ToArray();
            return dValues.covariance();
        }

        public static double[][] covariance(this double[][] values)
        {
            var count = values.Length - 1;
            var measures = values[0].Length;
            var means = new double[measures];
            var sums = new double[measures][];
            var totals = new double[measures][];

            for (var i = 0; i < measures; i++)
                means[i] = values.Average(v => v[i]);

            for (var i = 0; i < measures; i++)
            {
                sums[i] = new double[measures];
                totals[i] = new double[measures];

                for (var j = 0; j < measures; j++)
                {
                    sums[i][j] = values.Sum(v => (v[i] - means[i]) * (v[j] - means[j]));
                    totals[i][j] = sums[i][j] / count;
                }
            }

            return totals;
        }

        public static double[,] convertDimArray(this double[][] values)
        {
            var returnArray = new double[values.GetLength(0), values[0].GetLength(0)];
            for (var i = 0; i < values.Length; i++)
            {
                for (var j = 0; j < values[0].Length; j++)
                {
                    returnArray[i, j] = values[i][j];
                }
            }
            return returnArray;
        }

        public static double[] normalize(this IEnumerable<double> values, double minValue = 0.0, double maxValue = 1.0)
        {
            var max = values.Max();
            var min = values.Min();
            var delta = max - min;
            var rangeDelta = maxValue - minValue;

            return values
                .Select(v => (((v - min) / delta) * rangeDelta) + minValue)
                .ToArray();
        }

        public static IEnumerable<double> histogram(this IEnumerable<double> values, int steps = 100) => values.histogram(false, null, null, steps);

        public static IEnumerable<double> histogram(this IEnumerable<double> values, bool normalize = false, double? minValue = 0.0, double? maxValue = 1.0, int steps = 100)
        {
            var min = values.Min();
            var delta = (values.Max() - min);

            var counts = values.Select(v => Math.Min((int)(((v - min)/delta) * steps), steps - 1))
                .GroupBy(v => v)
                .Select(v => new {
                    index = v.Key,
                    count = v.Count()
                })
                .OrderBy(c => c.index);

            var _histogram = (
                    from e in Enumerable.Range(0, steps)
                    join c in counts
                        on e equals c.index into eg
                    from h in eg.DefaultIfEmpty()
                    select (double)(h?.count ?? 0)
                );


            return normalize ? _histogram.normalize(minValue.Value, maxValue.Value) : _histogram;
        }

        public static double norm(this IEnumerable<double> values)
            => Math.Sqrt(values.Sum(v => Math.Pow(v, 2.0)));

        public static double dot(this double[] values1, double[] values2)
        {
            var product = 0.0;
            for (int i = 0; i < values1.Length; i++)
                product += values1[i] * values2[i];

            return product;
        }
    }
}