using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class AuditEventsTests
{
    private readonly SmartShiftApiFactory _factory;

    public AuditEventsTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Hr_can_list_audit_events_after_mutation()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0827");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patch = await client.PatchAsJsonAsync("/api/v1/auth/me", new { preferredLanguage = "vi" });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var list = await client.GetFromJsonAsync<AuditPageResponse>(
            "/api/v1/audit-events?page=0&pageSize=20&sortBy=occurredAtUtc&sortDir=desc");
        Assert.NotNull(list);
        Assert.True(list!.TotalCount > 0);
        Assert.Contains(
            list.Items,
            e => e.Path.Contains("/api/v1/auth/me", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Staff_cannot_list_audit_events()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/audit-events?page=0&pageSize=5");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed class AuditPageResponse
    {
        [JsonPropertyName("items")]
        public List<AuditItem> Items { get; set; } = new();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    private sealed class AuditItem
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = "";
    }
}
