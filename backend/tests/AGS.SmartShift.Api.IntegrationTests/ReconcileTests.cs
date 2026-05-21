using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class ReconcileTests
{
    private readonly SmartShiftApiFactory _factory;

    public ReconcileTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task HR_can_load_reconcile_summary_and_export_week_month()
    {
        var hr = await CreateClientAsync("AGS0827");
        var weekId = await GetAnyWeekIdAsync(hr);

        var summaryResp = await hr.GetAsync($"/api/v1/reconcile?weekId={weekId}&departmentCode=PVHK_DI");
        summaryResp.EnsureSuccessStatusCode();
        var summary = await summaryResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(weekId, summary!.GetProperty("weekId").GetString());
        Assert.True(summary.TryGetProperty("employees", out _));

        var weekExport = await hr.GetAsync(
            $"/api/v1/reconcile/export/week?weekId={weekId}&departmentCode=PVHK_DI");
        weekExport.EnsureSuccessStatusCode();
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            weekExport.Content.Headers.ContentType?.MediaType);
        Assert.True((await weekExport.Content.ReadAsByteArrayAsync()).Length > 100);

        var monthExport = await hr.GetAsync(
            $"/api/v1/reconcile/export/month?weekId={weekId}&departmentCode=PVHK_DI");
        monthExport.EnsureSuccessStatusCode();
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            monthExport.Content.Headers.ContentType?.MediaType);
        Assert.True((await monthExport.Content.ReadAsByteArrayAsync()).Length > 100);
    }

    [Fact]
    public async Task Staff_cannot_access_reconcile()
    {
        var staff = await CreateClientAsync("AGS0138");
        var response = await staff.GetAsync("/api/v1/reconcile");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<string> GetAnyWeekIdAsync(HttpClient client)
    {
        var listResponse = await client.GetAsync("/api/v1/flights?page=1&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var listJson = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        return listJson!.GetProperty("items")[0].GetProperty("weekId").GetString()!;
    }

    private async Task<HttpClient> CreateClientAsync(string login)
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
