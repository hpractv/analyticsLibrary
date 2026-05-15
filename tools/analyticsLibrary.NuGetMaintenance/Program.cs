using System.Net;
using analyticsLibrary.ReleaseTooling;
using NuGet.Versioning;

var command = args.Length > 0 ? args[0] : string.Empty;

if (string.Equals(command, "apply-deprecations", StringComparison.Ordinal))
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: analyticsLibrary.NuGetMaintenance apply-deprecations <releaseVersion>");
        return 1;
    }

    var releaseRaw = args[1];
    var release = VersionDeprecationPlanner.TryParseReleaseVersion(releaseRaw);
    if (release is null)
    {
        Console.Error.WriteLine($"Invalid release version: {releaseRaw}");
        return 1;
    }

    var apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        Console.Error.WriteLine("NUGET_API_KEY is not set.");
        return 1;
    }

    using var http = new HttpClient();
    return await RunAsync(http, apiKey, release, CancellationToken.None).ConfigureAwait(false);
}
else if (string.Equals(command, "deprecate-betas", StringComparison.Ordinal))
{
    var apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        Console.Error.WriteLine("NUGET_API_KEY is not set.");
        return 1;
    }

    using var http = new HttpClient();
    return await DeprecateBetasAsync(http, apiKey, CancellationToken.None).ConfigureAwait(false);
}
else
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  analyticsLibrary.NuGetMaintenance apply-deprecations <releaseVersion>");
    Console.Error.WriteLine("  analyticsLibrary.NuGetMaintenance deprecate-betas");
    return 1;
}

static async Task<int> RunAsync(HttpClient http, string apiKey, NuGetVersion releaseVersion, CancellationToken cancellationToken)
{
    // 40 matches the NuGet Gallery API's per-request version limit for deprecation batches
    const int chunkSize = 40;
    foreach (var packageId in ReleaseConstants.PackageIds)
    {
        IReadOnlyList<string> published;
        try
        {
            published = await NuGetFlatContainerHttp.GetPublishedVersionsAsync(http, packageId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to list versions for {packageId}: {ex.Message}");
            return 1;
        }

        if (published.Count == 0)
        {
            Console.WriteLine($"No published versions found for {packageId}; skipping.");
            continue;
        }

        var betas = published.Where(VersionDeprecationPlanner.IsCiBetaPrerelease).ToList();
        var legacyStables = published
            .Where(v => VersionDeprecationPlanner.IsOlderStableThan(v, releaseVersion))
            .ToList();

        foreach (var chunk in Chunk(betas, chunkSize))
        {
            var code = await SendDeprecationAsync(
                    http,
                    apiKey,
                    packageId,
                    chunk,
                    isLegacy: false,
                    hasCriticalBugs: false,
                    isOther: true,
                    ReleaseConstants.PrereleaseTestingDeprecationMessage,
                    releaseVersion,
                    cancellationToken)
                .ConfigureAwait(false);
            if (code != 0)
            {
                return code;
            }
        }

        foreach (var chunk in Chunk(legacyStables, chunkSize))
        {
            var code = await SendDeprecationAsync(
                    http,
                    apiKey,
                    packageId,
                    chunk,
                    isLegacy: true,
                    hasCriticalBugs: false,
                    isOther: false,
                    ReleaseConstants.LegacyDeprecationMessage,
                    releaseVersion,
                    cancellationToken)
                .ConfigureAwait(false);
            if (code != 0)
            {
                return code;
            }
        }
    }

    return 0;
}

static async Task<int> DeprecateBetasAsync(HttpClient http, string apiKey, CancellationToken cancellationToken)
{
    // 40 matches the NuGet Gallery API's per-request version limit for deprecation batches
    const int chunkSize = 40;
    foreach (var packageId in ReleaseConstants.PackageIds)
    {
        IReadOnlyList<string> published;
        try
        {
            published = await NuGetFlatContainerHttp.GetPublishedVersionsAsync(http, packageId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to list versions for {packageId}: {ex.Message}");
            return 1;
        }

        if (published.Count == 0)
        {
            Console.WriteLine($"No published versions found for {packageId}; skipping.");
            continue;
        }

        var betas = published.Where(VersionDeprecationPlanner.IsCiBetaPrerelease).ToList();
        foreach (var chunk in Chunk(betas, chunkSize))
        {
            var code = await SendDeprecationAsync(
                    http,
                    apiKey,
                    packageId,
                    chunk,
                    isLegacy: false,
                    hasCriticalBugs: false,
                    isOther: true,
                    ReleaseConstants.PrereleaseTestingDeprecationMessage,
                    alternateVersion: null,
                    cancellationToken)
                .ConfigureAwait(false);
            if (code != 0)
            {
                return code;
            }
        }
    }

    return 0;
}

static List<List<string>> Chunk(IReadOnlyList<string> items, int size)
{
    var chunks = new List<List<string>>();
    for (var i = 0; i < items.Count; i += size)
    {
        var end = Math.Min(i + size, items.Count);
        var chunk = new List<string>(end - i);
        for (var j = i; j < end; j++)
        {
            chunk.Add(items[j]);
        }

        chunks.Add(chunk);
    }

    return chunks;
}

static async Task<int> SendDeprecationAsync(
    HttpClient http,
    string apiKey,
    string packageId,
    IReadOnlyList<string> versions,
    bool isLegacy,
    bool hasCriticalBugs,
    bool isOther,
    string message,
    NuGetVersion? alternateVersion,
    CancellationToken cancellationToken)
{
    if (versions.Count == 0)
    {
        return 0;
    }

    var normalizedRelease = alternateVersion?.ToNormalizedString();
    using var request = NuGetGalleryDeprecationApi.CreateDeprecationRequest(
        packageId,
        versions,
        isLegacy,
        hasCriticalBugs,
        isOther,
        message,
        normalizedRelease is not null ? packageId : null,
        normalizedRelease,
        apiKey);

    using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine(
            $"Deprecated {versions.Count} version(s) of {packageId} (legacy={isLegacy}, other={isOther}).");
        return 0;
    }

    var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    Console.Error.WriteLine(
        $"NuGet deprecation failed for {packageId}: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");

    if (response.StatusCode == HttpStatusCode.Forbidden)
    {
        Console.Error.WriteLine(
            "Hint: ensure the API key allows package push/unlist and that the NuGet.org account has the manage-deprecation API enabled for this key.");
    }

    return 1;
}
