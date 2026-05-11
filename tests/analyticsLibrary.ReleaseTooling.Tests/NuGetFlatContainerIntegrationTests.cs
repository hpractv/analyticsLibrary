using analyticsLibrary.ReleaseTooling;

namespace analyticsLibrary.ReleaseTooling.Tests;

[Trait("Category", "Integration")]
public class NuGetFlatContainerIntegrationTests
{
    [Fact]
    public async Task Flat_container_index_for_core_has_versions_array()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        Exception? last = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var json = await http.GetStringAsync(NuGetFlatContainerHttp.GetPackageIndexUri("analyticsLibrary.Core"));
                var versions = FlatContainerIndexParser.ParseVersions(json);
                Assert.NotEmpty(versions);
                return;
            }
            catch (Exception ex)
            {
                last = ex;
                if (attempt < 2)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        throw last ?? new InvalidOperationException("Flat container request failed.");
    }
}
