using analyticsLibrary.ReleaseTooling;
using NuGet.Versioning;

namespace analyticsLibrary.ReleaseTooling.Tests;

public class VersionDeprecationPlannerTests
{
    [Theory]
    [InlineData("2.0.0-beta.pr-6.2", true)]
    [InlineData("2.0.0-beta", false)]
    [InlineData("2.0.0", false)]
    [InlineData("2.0.0-rc.1", false)]
    public void IsCiBetaPrerelease_matches_pr_build_suffix(string version, bool expected) =>
        Assert.Equal(expected, VersionDeprecationPlanner.IsCiBetaPrerelease(version));

    [Fact]
    public void IsOlderStableThan_excludes_prerelease_and_current()
    {
        var rel = NuGetVersion.Parse("3.0.0");
        Assert.True(VersionDeprecationPlanner.IsOlderStableThan("2.0.0", rel));
        Assert.False(VersionDeprecationPlanner.IsOlderStableThan("3.0.0", rel));
        Assert.False(VersionDeprecationPlanner.IsOlderStableThan("3.0.1", rel));
        Assert.False(VersionDeprecationPlanner.IsOlderStableThan("3.0.0-beta.pr-1.1", rel));
    }

    [Fact]
    public void Legacy_message_constant_is_exact()
    {
        Assert.Equal(
            "This package is legacy and is no longer maintained",
            ReleaseConstants.LegacyDeprecationMessage);
    }
}
