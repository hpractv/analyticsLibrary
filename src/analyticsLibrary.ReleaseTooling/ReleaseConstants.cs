namespace analyticsLibrary.ReleaseTooling;

public static class ReleaseConstants
{
    public const string LegacyDeprecationMessage =
        "This package is legacy and is no longer maintained";

    public const string PrereleaseTestingDeprecationMessage =
        "This pre-release package was published for pull request validation only. Install the latest stable release instead.";

    public static readonly string[] PackageIds =
    [
        "analyticsLibrary.Core",
        "analyticsLibrary.Algorithms",
        "analyticsLibrary.Statistics",
        "analyticsLibrary.Excel",
        "analyticsLibrary.Access",
        "analyticsLibrary.Hadoop",
        "analyticsLibrary",
    ];

    public const string PrereleaseLabelPrefix = "-beta.pr-";
}
