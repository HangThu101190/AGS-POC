using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class AuthSecurityTests
{
    private readonly SmartShiftApiFactory _factory;

    public AuthSecurityTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_with_weak_password_returns_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { login = "AGS0138", password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
