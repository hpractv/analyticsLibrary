namespace analyticsLibrary.ReleaseTooling;

public static class NuGetGalleryDeprecationApi
{
    public const string DeprecationEndpointTemplate = "https://www.nuget.org/api/v2/package/{0}/deprecations";

    public static HttpRequestMessage CreateDeprecationRequest(
        string packageId,
        IReadOnlyList<string> versions,
        bool isLegacy,
        bool hasCriticalBugs,
        bool isOther,
        string? customMessage,
        string? alternatePackageId,
        string? alternatePackageVersion,
        string apiKey)
    {
        if (versions.Count == 0)
        {
            throw new ArgumentException("At least one version is required.", nameof(versions));
        }

        var hasAltId = !string.IsNullOrEmpty(alternatePackageId);
        var hasAltVersion = !string.IsNullOrEmpty(alternatePackageVersion);
        if (hasAltId != hasAltVersion)
        {
            throw new ArgumentException(
                "alternatePackageId and alternatePackageVersion must both be provided or both be omitted.");
        }

        var pairs = new List<KeyValuePair<string, string>>();
        for (var i = 0; i < versions.Count; i++)
        {
            pairs.Add(new KeyValuePair<string, string>($"versions[{i}]", versions[i]));
        }

        if (isLegacy)
        {
            pairs.Add(new KeyValuePair<string, string>("isLegacy", "true"));
        }

        if (hasCriticalBugs)
        {
            pairs.Add(new KeyValuePair<string, string>("hasCriticalBugs", "true"));
        }

        if (isOther)
        {
            pairs.Add(new KeyValuePair<string, string>("isOther", "true"));
        }

        if (!string.IsNullOrEmpty(customMessage))
        {
            pairs.Add(new KeyValuePair<string, string>("message", customMessage));
        }

        if (!string.IsNullOrEmpty(alternatePackageId))
        {
            pairs.Add(new KeyValuePair<string, string>("alternatePackageId", alternatePackageId));
        }

        if (!string.IsNullOrEmpty(alternatePackageVersion))
        {
            pairs.Add(new KeyValuePair<string, string>("alternatePackageVersion", alternatePackageVersion));
        }
        pairs.Add(new KeyValuePair<string, string>("listedVerb", "Unchanged"));

        var url = string.Format(DeprecationEndpointTemplate, Uri.EscapeDataString(packageId));
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.TryAddWithoutValidation("X-NuGet-ApiKey", apiKey);
        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            "analyticsLibrary-release-workflow/1.0 (+https://github.com/hpractv/analyticsLibrary)");
        request.Content = new FormUrlEncodedContent(pairs);
        return request;
    }
}
