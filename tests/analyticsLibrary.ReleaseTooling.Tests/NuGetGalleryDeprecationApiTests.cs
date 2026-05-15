using System.Net.Http;
using analyticsLibrary.ReleaseTooling;

namespace analyticsLibrary.ReleaseTooling.Tests;

public class NuGetGalleryDeprecationApiTests
{
    [Fact]
    public void CreateDeprecationRequest_uses_put_form_urlencoded()
    {
        using var req = NuGetGalleryDeprecationApi.CreateDeprecationRequest(
            "Some.Package",
            new[] { "1.0.0", "1.0.1" },
            isLegacy: true,
            hasCriticalBugs: false,
            isOther: false,
            "msg",
            "Some.Package",
            "2.0.0",
            "fake-key");

        Assert.Equal(HttpMethod.Put, req.Method);
        Assert.Contains("Some.Package", req.RequestUri!.AbsoluteUri, StringComparison.OrdinalIgnoreCase);
        Assert.True(req.Headers.Contains("X-NuGet-ApiKey"));
        Assert.NotNull(req.Content);
        Assert.Equal("application/x-www-form-urlencoded", req.Content.Headers.ContentType!.MediaType);
    }
}
