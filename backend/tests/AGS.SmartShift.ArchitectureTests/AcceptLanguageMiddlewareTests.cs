using AGS.SmartShift.Api.Middleware;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace AGS.SmartShift.ArchitectureTests;

public sealed class AcceptLanguageMiddlewareTests
{
    [Theory]
    [InlineData(null, "vi")]
    [InlineData("", "vi")]
    [InlineData("vi-VN,vi;q=0.9", "vi")]
    [InlineData("en-US,en;q=0.9,vi;q=0.8", "en")]
    [InlineData("fr-FR,en;q=0.8", "en")]
    [InlineData("fr-FR,de;q=0.9", "vi")]
    public void ResolveLanguage_parses_supported_tags(string? header, string expected)
    {
        StringValues values = header ?? StringValues.Empty;
        var actual = AcceptLanguageMiddleware.ResolveLanguage(values);
        Assert.Equal(expected, actual);
    }
}
