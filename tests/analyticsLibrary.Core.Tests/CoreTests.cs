using System;
using System.IO;
using System.Linq;
using analyticsLibrary.Core;
using Xunit;

namespace analyticsLibrary.Core.Tests
{
    public class AnalyticsTests
    {
        [Fact]
        public void Compress_RemovesConsecutiveDuplicateChars()
        {
            Assert.Equal("abcd", "aabbccdd".compress());
        }

        [Fact]
        public void Compress_WithPattern_CollapsesRepeatedDelimiter()
        {
            Assert.Equal("a,b,c", "a,,b,,c".compress(","));
        }

        [Fact]
        public void TitleCase_CapitalizesFirstLetterOfEachWord()
        {
            Assert.Equal("Hello World", "hello world".titleCase());
        }

        [Fact]
        public void WordReplace_SubstitutesPattern()
        {
            Assert.Equal("foo baz", "foo bar".wordReplace("bar", "baz"));
        }
    }

    public class DataTypeHelperTests
    {
        [Theory]
        [InlineData("int", dataTypeEnum.intType)]
        [InlineData("INT", dataTypeEnum.intType)]
        [InlineData("varchar", dataTypeEnum.varcharType)]
        [InlineData("varchar2", dataTypeEnum.varcharType)]
        [InlineData("nvarchar", dataTypeEnum.nvarcharType)]
        [InlineData("float", dataTypeEnum.floatType)]
        [InlineData("decimal", dataTypeEnum.decimalType)]
        [InlineData("bigint", dataTypeEnum.bigintType)]
        [InlineData("date", dataTypeEnum.dateType)]
        [InlineData("datetime", dataTypeEnum.datetimeType)]
        [InlineData("bit", dataTypeEnum.bitType)]
        [InlineData("char", dataTypeEnum.charType)]
        [InlineData("money", dataTypeEnum.moneyType)]
        [InlineData("numeric", dataTypeEnum.numericType)]
        [InlineData("number", dataTypeEnum.numericType)]
        [InlineData("long", dataTypeEnum.numericType)]
        [InlineData("tinyint", dataTypeEnum.tinyintType)]
        [InlineData("smalldatetime", dataTypeEnum.smalldatetimeType)]
        [InlineData("timestamp", dataTypeEnum.timestampType)]
        [InlineData("timestamp(3)", dataTypeEnum.timestampType)]
        [InlineData("varbinary", dataTypeEnum.varbinaryType)]
        public void DataTypeFromString_ReturnsExpectedEnum(string input, dataTypeEnum expected)
        {
            Assert.Equal(expected, dataTypeHelper.dataTypeFromString(input));
        }

        [Fact]
        public void DataTypeFromString_UnknownInput_ReturnsUnknown()
        {
            Assert.Equal(dataTypeEnum.unknown, dataTypeHelper.dataTypeFromString("notatype"));
        }
    }

    public class ExtensionsDateTests
    {
        [Fact]
        public void FromSasEpochDate_ValidDate_ParsesCorrectly()
        {
            // SAS date format: ddMMMyyyy:hh:mm:ss.fff
            var result = "01JAN2020:00:00:00.000".fromSasEpochDate();
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2020, 1, 1), result!.Value.Date);
        }

        [Fact]
        public void FromSasEpochDate_NullInput_ReturnsNull()
        {
            string? value = null;
            Assert.Null(value!.fromSasEpochDate());
        }

        [Fact]
        public void FromSasEpochDate_WhitespaceInput_ReturnsNull()
        {
            Assert.Null("   ".fromSasEpochDate());
        }
    }

    public class CsvRoundTripTests
    {
        [Fact]
        public void WriteCsv_ThenRead_PreservesData()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var header = new[] { "Name", "Age", "City" };
                var rows = new[]
                {
                    new object[] { "Alice", 30, "New York" },
                    new object[] { "Bob", 25, "Los Angeles" },
                };

                rows.writeCsv(tempFile, header, ',');

                var csvReader = new Csv(tempFile);
                var data = csvReader.data.ToList();

                Assert.Equal(2, data.Count);
                Assert.Equal("Alice", data[0].values[0]?.ToString());
                Assert.Equal("30", data[0].values[1]?.ToString());
                Assert.Equal("Bob", data[1].values[0]?.ToString());
                Assert.Equal("Los Angeles", data[1].values[2]?.ToString());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void FromCsv_ParsesDelimitedLine()
        {
            var parts = "one,two,three".fromCsv();
            Assert.Equal(3, parts.Length);
            Assert.Equal("one", parts[0]);
            Assert.Equal("two", parts[1]);
            Assert.Equal("three", parts[2]);
        }

        [Fact]
        public void ToCsv_JoinsObjectsWithComma()
        {
            var parts = new object[] { "alpha", "beta", "gamma" };
            Assert.Equal("alpha,beta,gamma", parts.toCsv());
        }
    }
}
