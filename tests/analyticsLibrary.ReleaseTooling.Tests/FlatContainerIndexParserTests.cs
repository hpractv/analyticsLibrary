using analyticsLibrary.ReleaseTooling;

namespace analyticsLibrary.ReleaseTooling.Tests;

public class FlatContainerIndexParserTests
{
    [Fact]
    public void ParseVersions_extracts_ordered_strings()
    {
        const string json = """
            {"versions":["1.0.0","2.0.0-beta.pr-1.2"]}
            """;

        var v = FlatContainerIndexParser.ParseVersions(json);

        Assert.Equal(new[] { "1.0.0", "2.0.0-beta.pr-1.2" }, v);
    }

    [Fact]
    public void ParseVersions_missing_array_returns_empty()
    {
        const string json = """{"other":[]}""";

        Assert.Empty(FlatContainerIndexParser.ParseVersions(json));
    }
}
