using System.Net;
using System.Net.Http;
using analyticsLibrary.ReleaseTooling;

namespace analyticsLibrary.ReleaseTooling.Tests;

public class NuGetFlatContainerIntegrationTests
{
    [Fact]
    public async Task Flat_container_index_for_core_has_versions_array()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"versions":["1.0.0","2.0.0"]}""")
            });
        using var http = new HttpClient(handler);
        var versions = await NuGetFlatContainerHttp.GetPublishedVersionsAsync(http, "analyticsLibrary.Core");
        Assert.NotEmpty(versions);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handler(request));
    }
}
