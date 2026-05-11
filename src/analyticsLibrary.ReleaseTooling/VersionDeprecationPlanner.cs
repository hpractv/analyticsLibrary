using NuGet.Versioning;

namespace analyticsLibrary.ReleaseTooling;

public static class VersionDeprecationPlanner
{
    public static bool IsCiBetaPrerelease(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return false;
        }

        if (!NuGetVersion.TryParse(versionString, out var v) || !v.IsPrerelease)
        {
            return false;
        }

        return versionString.Contains(ReleaseConstants.PrereleaseLabelPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOlderStableThan(string versionString, NuGetVersion releaseVersion)
    {
        if (!NuGetVersion.TryParse(versionString, out var v))
        {
            return false;
        }

        if (v.IsPrerelease)
        {
            return false;
        }

        return v < releaseVersion;
    }

    public static NuGetVersion? TryParseReleaseVersion(string versionString) =>
        NuGetVersion.TryParse(versionString, out var v) ? v : null;
}
