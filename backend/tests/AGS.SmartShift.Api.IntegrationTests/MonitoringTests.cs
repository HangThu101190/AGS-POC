using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class MonitoringTests
{
    private readonly SmartShiftApiFactory _factory;

    public MonitoringTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task TC_MON_001_Hr_can_load_monitoring_snapshot()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0827");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/monitoring/snapshot");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("generatedAtUtc", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("markers", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TC_MON_002_Snapshot_since_returns_304_when_unchanged()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0827");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var first = await client.GetAsync("/api/v1/monitoring/snapshot");
        first.EnsureSuccessStatusCode();
        var body = await first.Content.ReadFromJsonAsync<SnapshotWire>();
        Assert.NotNull(body?.GeneratedAtUtc);

        var since = Uri.EscapeDataString(body.GeneratedAtUtc.ToString("O"));
        var second = await client.GetAsync($"/api/v1/monitoring/snapshot?since={since}");

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task TC_MON_003_Staff_without_checkin_rejects_location_samples()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0139");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            samples = new[] { new { lat = 11.9982, lng = 109.2191 } },
        };
        var response = await client.PostAsJsonAsync("/api/v1/attendance/location-samples", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC_MON_004_Staff_with_checkin_accepts_location_samples()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var checkIn = await client.PostAsJsonAsync(
            "/api/v1/attendance/check-in",
            new { lat = 11.99825, lng = 109.21925, confirmOutsideZone = false });
        if (checkIn.StatusCode == HttpStatusCode.Conflict)
        {
            await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { note = "reset for TC_MON_004" });
            checkIn = await client.PostAsJsonAsync(
                "/api/v1/attendance/check-in",
                new { lat = 11.99825, lng = 109.21925, confirmOutsideZone = false });
        }

        checkIn.EnsureSuccessStatusCode();

        var body = new
        {
            samples = new[] { new { lat = 11.9982, lng = 109.2191 } },
        };
        var response = await client.PostAsJsonAsync("/api/v1/attendance/location-samples", body);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<LocationSamplesWire>();
        Assert.NotNull(payload);
        Assert.True(payload!.Accepted >= 1);
    }

    private sealed class SnapshotWire
    {
        public DateTime GeneratedAtUtc { get; init; }
    }

    private sealed class LocationSamplesWire
    {
        public int Accepted { get; init; }
    }
}
