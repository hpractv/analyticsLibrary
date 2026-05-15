namespace analyticsLibrary.ReleaseTooling;

public static class NuGetFlatContainerHttp
{
    public static Uri GetPackageIndexUri(string packageId) =>
        new($"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json", UriKind.Absolute);

    public static async Task<IReadOnlyList<string>> GetPublishedVersionsAsync(
        HttpClient http,
        string packageId,
        CancellationToken cancellationToken = default)
    {
        var uri = GetPackageIndexUri(packageId);
        using var response = await http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Array.Empty<string>();
        }

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return FlatContainerIndexParser.ParseVersions(json);
    }
}
