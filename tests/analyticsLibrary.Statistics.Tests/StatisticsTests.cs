using System;
using System.Linq;
using analyticsLibrary.Statistics;
using Xunit;

namespace analyticsLibrary.Statistics.Tests
{
    public class StandardDeviationTests
    {
        // Population: {2, 4, 4, 4, 5, 5, 7, 9} — population stddev = 2.0
        private static readonly double[] KnownValues = { 2, 4, 4, 4, 5, 5, 7, 9 };

        [Fact]
        public void StandardDeviation_Double_ReturnsCorrectValue()
        {
            var result = KnownValues.standardDeviation();
            Assert.Equal(2.0, result, precision: 10);
        }

        [Fact]
        public void StandardDeviation_Int_ReturnsCorrectValue()
        {
            var intValues = KnownValues.Select(v => (int)v).ToArray();
            Assert.Equal(2.0, intValues.standardDeviation(), precision: 10);
        }

        [Fact]
        public void Variance_IsSquareOfStandardDeviation()
        {
            var stddev = KnownValues.standardDeviation();
            var variance = KnownValues.variance();
            Assert.Equal(stddev * stddev, variance, precision: 10);
        }

        [Fact]
        public void Variance_Int_ReturnsCorrectValue()
        {
            var intValues = KnownValues.Select(v => (int)v).ToArray();
            Assert.Equal(4.0, intValues.variance(), precision: 10);
        }
    }

    public class NormalizeTests
    {
        [Fact]
        public void Normalize_MapsMinToZeroAndMaxToOne()
        {
            var values = new double[] { 1, 2, 3, 4, 5 };
            var normalized = values.normalize();
            Assert.Equal(0.0, normalized.Min(), precision: 10);
            Assert.Equal(1.0, normalized.Max(), precision: 10);
        }

        [Fact]
        public void Normalize_CustomRange_MapsToRange()
        {
            var values = new double[] { 0, 50, 100 };
            var normalized = values.normalize(0, 10);
            Assert.Equal(0.0, normalized[0], precision: 10);
            Assert.Equal(5.0, normalized[1], precision: 10);
            Assert.Equal(10.0, normalized[2], precision: 10);
        }

        [Fact]
        public void Normalize_PreservesCount()
        {
            var values = new double[] { 3, 1, 4, 1, 5, 9 };
            Assert.Equal(values.Length, values.normalize().Length);
        }
    }

    public class HistogramTests
    {
        [Fact]
        public void Histogram_Default100Steps_ReturnsBuckets()
        {
            var values = Enumerable.Range(0, 200).Select(i => (double)i).ToArray();
            var hist = values.histogram(steps: 100).ToArray();
            Assert.Equal(100, hist.Length);
        }

        [Fact]
        public void Histogram_AllBucketsNonNegative()
        {
            var values = new double[] { 1, 2, 3, 4, 5, 10, 20 };
            Assert.All(values.histogram(steps: 5), v => Assert.True(v >= 0));
        }

        [Fact]
        public void Histogram_TotalCountEqualsInputCount()
        {
            var values = new double[] { 1, 2, 3, 4, 5 };
            var total = values.histogram(steps: 5).Sum();
            Assert.Equal(values.Length, (int)total);
        }
    }

    public class CovarianceTests
    {
        // The covariance function treats the jagged array as rows=observations, cols=variables.
        // The result is a [variables x variables] matrix using sample covariance (n-1 denominator).

        [Fact]
        public void Covariance_TwoVariables_DiagonalIsSampleVariance()
        {
            // 3 observations of 2 perfectly-correlated variables
            var matrix = new double[][] {
                new double[] { 1, 4 },
                new double[] { 2, 5 },
                new double[] { 3, 6 },
            };
            var cov = matrix.covariance();
            Assert.Equal(2, cov.Length);
            Assert.Equal(1.0, cov[0][0], precision: 10);
            Assert.Equal(1.0, cov[1][1], precision: 10);
        }

        [Fact]
        public void Covariance_Int_ReturnsSquareMatrixWithVariableCount()
        {
            // 3 observations of 2 variables
            var matrix = new int[][] {
                new int[] { 1, 4 },
                new int[] { 2, 5 },
                new int[] { 3, 6 },
            };
            var result = matrix.covariance();
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(2, result[0].Length);
        }
    }

    public class DotProductTests
    {
        [Fact]
        public void Dot_TwoKnownVectors_ReturnsCorrectScalar()
        {
            // [1,2,3] · [4,5,6] = 4 + 10 + 18 = 32
            var a = new double[] { 1, 2, 3 };
            var b = new double[] { 4, 5, 6 };
            Assert.Equal(32.0, a.dot(b), precision: 10);
        }

        [Fact]
        public void Dot_OrthogonalVectors_ReturnsZero()
        {
            var a = new double[] { 1, 0 };
            var b = new double[] { 0, 1 };
            Assert.Equal(0.0, a.dot(b), precision: 10);
        }
    }
}
