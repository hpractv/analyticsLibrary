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

    [Fact]
    public void CreateDeprecationRequest_allows_both_alternate_fields_omitted()
    {
        using var req = NuGetGalleryDeprecationApi.CreateDeprecationRequest(
            "Some.Package",
            new[] { "1.0.0" },
            isLegacy: false,
            hasCriticalBugs: false,
            isOther: true,
            "msg",
            alternatePackageId: null,
            alternatePackageVersion: null,
            "fake-key");

        Assert.NotNull(req);
    }

    [Theory]
    [InlineData("Some.Package", null)]
    [InlineData(null, "2.0.0")]
    public void CreateDeprecationRequest_throws_when_only_one_alternate_field_is_provided(
        string? altId, string? altVersion)
    {
        Assert.Throws<ArgumentException>(() =>
            NuGetGalleryDeprecationApi.CreateDeprecationRequest(
                "Some.Package",
                new[] { "1.0.0" },
                isLegacy: false,
                hasCriticalBugs: false,
                isOther: true,
                "msg",
                altId,
                altVersion,
                "fake-key"));
    }
}
