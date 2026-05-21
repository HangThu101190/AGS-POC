using System.Text.Json;
using System.Text.Json.Serialization;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AGS.SmartShift.Api.IntegrationTests;

public sealed class SmartShiftApiFactory : WebApplicationFactory<Program>
{
    private readonly string _inMemoryDatabaseName = $"SmartShiftTests_{Guid.NewGuid():N}";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["InMemoryDatabaseName"] = _inMemoryDatabaseName,
                ["ConnectionStrings:Default"] = string.Empty,
                ["Jwt:SigningKey"] = "integration-test-signing-key-32chars-min!",
                ["Jwt:Issuer"] = "AGS.SmartShift.Test",
                ["Jwt:Audience"] = "AGS.SmartShift.Test",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
            });
        });
    }

    public async Task<string> LoginAsync(HttpClient client, string employeeCode)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { login = employeeCode, password = AuthSeedData.DevPassword });

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<AuthSessionResponse>(JsonOptions);
        if (string.IsNullOrWhiteSpace(session?.AccessToken))
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login response missing accessToken. Body: {body}");
        }

        return session.AccessToken;
    }
}

internal sealed class AuthSessionResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
}
