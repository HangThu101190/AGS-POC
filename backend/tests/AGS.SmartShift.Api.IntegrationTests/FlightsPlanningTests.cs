using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class FlightsPlanningTests
{
    private readonly SmartShiftApiFactory _factory;

    public FlightsPlanningTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Tbdh_can_list_flights_and_toggle_delay()
    {
        var client = await CreateClientAsync("AGS0901");
        var listResponse = await client.GetAsync("/api/v1/flights?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var json = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("items");
        Assert.True(items.GetArrayLength() > 0);
        var flightId = items[0].GetProperty("id").GetGuid();

        var delayResponse = await client.PatchAsJsonAsync(
            $"/api/v1/flights/{flightId}/delay",
            new { delayMinutes = 75 });
        Assert.Equal(HttpStatusCode.OK, delayResponse.StatusCode);

        var updated = await delayResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(75, updated.GetProperty("delayMinutes").GetInt32());
        Assert.True(updated.GetProperty("isDelayed").GetBoolean());
    }

    [Fact]
    public async Task Tbdh_can_set_split_eta_etd_delay()
    {
        var client = await CreateClientAsync("AGS0901");
        var listResponse = await client.GetAsync("/api/v1/flights?page=1&pageSize=20");
        listResponse.EnsureSuccessStatusCode();
        var json = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var flightId = json.GetProperty("items")[0].GetProperty("id").GetGuid();

        var delayResponse = await client.PatchAsJsonAsync(
            $"/api/v1/flights/{flightId}/delay",
            new { etaDelayMinutes = 35, etdDelayMinutes = 0 });
        delayResponse.EnsureSuccessStatusCode();

        var updated = await delayResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(35, updated.GetProperty("etaDelayMinutes").GetInt32());
        Assert.Equal(0, updated.GetProperty("etdDelayMinutes").GetInt32());
        Assert.Equal(35, updated.GetProperty("delayMinutes").GetInt32());
        Assert.True(updated.TryGetProperty("eta", out var eta) && eta.GetString()?.Contains("01:10") == true);
    }

    [Fact]
    public async Task Tbdh_can_import_reference_excel()
    {
        var excelPath = FindReferenceFlightPlanExcel();
        var client = await CreateClientAsync("AGS0901");
        await using var stream = File.OpenRead(excelPath);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", Path.GetFileName(excelPath));

        var response = await client.PostAsync("/api/v1/flights/import", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("importedCount").GetInt32() > 0);
    }

    [Fact]
    public async Task Import_rejects_file_with_xlsx_extension_but_invalid_content()
    {
        var client = await CreateClientAsync("AGS0901");
        var bytes = "not an excel file"u8.ToArray();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "fake.xlsx");

        var response = await client.PostAsync("/api/v1/flights/import", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Excel", body.GetProperty("title").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Staff_cannot_import_flights_or_delay()
    {
        var client = await CreateClientAsync("AGS0138");
        var excelPath = FindReferenceFlightPlanExcel();
        await using var stream = File.OpenRead(excelPath);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", Path.GetFileName(excelPath));

        var importResponse = await client.PostAsync("/api/v1/flights/import", content);
        Assert.Equal(HttpStatusCode.Forbidden, importResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/flights?page=0&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var flightId = (await listResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("items")[0]
            .GetProperty("id")
            .GetGuid();

        var delayResponse = await client.PatchAsJsonAsync(
            $"/api/v1/flights/{flightId}/delay",
            new { delayMinutes = 75 });
        Assert.Equal(HttpStatusCode.Forbidden, delayResponse.StatusCode);
    }

    [Fact]
    public async Task Hr_cannot_import_flights_or_delay()
    {
        var client = await CreateClientAsync("AGS0827");
        var excelPath = FindReferenceFlightPlanExcel();
        await using var stream = File.OpenRead(excelPath);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", Path.GetFileName(excelPath));

        var importResponse = await client.PostAsync("/api/v1/flights/import", content);
        Assert.Equal(HttpStatusCode.Forbidden, importResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/flights?page=0&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var flightId = (await listResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("items")[0]
            .GetProperty("id")
            .GetGuid();

        var delayResponse = await client.PatchAsJsonAsync(
            $"/api/v1/flights/{flightId}/delay",
            new { delayMinutes = 75 });
        Assert.Equal(HttpStatusCode.Forbidden, delayResponse.StatusCode);
    }

    [Fact]
    public async Task Tbdh_cannot_delay_flight_on_past_day()
    {
        var client = await CreateClientAsync("AGS0901");
        var listResponse = await client.GetAsync("/api/v1/flights?page=0&pageSize=50");
        listResponse.EnsureSuccessStatusCode();
        var listJson = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var weekId = Uri.EscapeDataString(
            listJson.GetProperty("items")[0].GetProperty("weekId").GetString()!);

        var planResponse = await client.GetAsync($"/api/v1/plans/{weekId}");
        planResponse.EnsureSuccessStatusCode();
        var plan = await planResponse.Content.ReadFromJsonAsync<JsonElement>();
        var todayIdx = plan.GetProperty("todayIdx").GetInt32();
        if (todayIdx <= 0)
        {
            return;
        }

        var pastFlightId = Guid.Empty;
        foreach (var item in listJson.GetProperty("items").EnumerateArray())
        {
            if (item.GetProperty("dayIdx").GetInt32() < todayIdx)
            {
                pastFlightId = item.GetProperty("id").GetGuid();
                break;
            }
        }

        if (pastFlightId == Guid.Empty)
        {
            return;
        }

        var delayResponse = await client.PatchAsJsonAsync(
            $"/api/v1/flights/{pastFlightId}/delay",
            new { delayMinutes = 75 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delayResponse.StatusCode);
    }

    [Fact]
    public async Task Staff_can_read_weekly_plan()
    {
        var client = await CreateClientAsync("AGS0138");
        var listResponse = await client.GetAsync("/api/v1/flights?page=0&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var weekId = Uri.EscapeDataString(
            (await listResponse.Content.ReadFromJsonAsync<JsonElement>())
                .GetProperty("items")[0]
                .GetProperty("weekId")
                .GetString()!);

        var planResponse = await client.GetAsync($"/api/v1/plans/{weekId}");
        Assert.Equal(HttpStatusCode.OK, planResponse.StatusCode);

        var generateResponse = await client.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        Assert.Equal(HttpStatusCode.Forbidden, generateResponse.StatusCode);
    }

    [Fact]
    public async Task Hr_can_read_but_not_mutate_weekly_plan()
    {
        var client = await CreateClientAsync("AGS0827");
        var weekId = await GetAnyWeekIdAsync(client);

        var planResponse = await client.GetAsync($"/api/v1/plans/{weekId}");
        Assert.Equal(HttpStatusCode.OK, planResponse.StatusCode);

        var generate = await client.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        Assert.Equal(HttpStatusCode.Forbidden, generate.StatusCode);
    }

    [Fact]
    public async Task Sup_can_generate_and_publish_plan()
    {
        var client = await CreateClientAsync("AGS0184");
        var weekId = await GetAnyWeekIdAsync(client);

        var generate = await client.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        Assert.Equal(HttpStatusCode.OK, generate.StatusCode);

        var publish = await client.PostAsync($"/api/v1/plans/{weekId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var plan = await publish.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("published", plan.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Tbdh_can_crud_flight_then_publish_locks_schedule()
    {
        var client = await CreateClientAsync("AGS0901");
        var weekId = Uri.UnescapeDataString(await GetAnyWeekIdAsync(client));

        var planResponse = await client.GetAsync($"/api/v1/plans/{Uri.EscapeDataString(weekId)}");
        planResponse.EnsureSuccessStatusCode();
        var plan = await planResponse.Content.ReadFromJsonAsync<JsonElement>();
        var todayIdx = plan.GetProperty("todayIdx").GetInt32();

        var upsert = new
        {
            weekId,
            dayIdx = todayIdx,
            flightNo = "TST999",
            route = "TST-CXR",
            sta = "14:00",
            std = "15:00",
            departmentCode = "PVHK_DI",
            manning = 2,
        };

        var create = await client.PostAsJsonAsync("/api/v1/flights", upsert);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var flightId = created.GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync(
            $"/api/v1/flights/{flightId}",
            new { upsert.flightNo, upsert.route, upsert.sta, upsert.std, upsert.departmentCode, upsert.manning, dayIdx = todayIdx });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/v1/flights/{flightId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var publish = await client.PostAsync($"/api/v1/flight-schedules/{weekId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var locked = await publish.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(locked.GetProperty("isLocked").GetBoolean());

        var blocked = await client.PostAsJsonAsync("/api/v1/flights", upsert);
        Assert.Equal(HttpStatusCode.BadRequest, blocked.StatusCode);
    }

    [Fact]
    public async Task Sup_first_assignment_sets_plan_in_progress()
    {
        var client = await CreateClientAsync("AGS0184");
        var weekId = await GetAnyWeekIdAsync(client);

        await client.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        await client.PostAsync($"/api/v1/plans/{weekId}/publish", null);

        var planResponse = await client.GetAsync($"/api/v1/plans/{weekId}");
        planResponse.EnsureSuccessStatusCode();
        var plan = await planResponse.Content.ReadFromJsonAsync<JsonElement>();
        var todayIdx = plan.GetProperty("todayIdx").GetInt32();

        var slotId = Guid.Empty;
        foreach (var slot in plan.GetProperty("slots").EnumerateArray())
        {
            if (slot.GetProperty("dayIdx").GetInt32() == todayIdx
                && slot.GetProperty("departmentCode").GetString() == "PVHK_DI")
            {
                slotId = slot.GetProperty("id").GetGuid();
                break;
            }
        }

        Assert.NotEqual(Guid.Empty, slotId);

        var assign = await client.PostAsJsonAsync(
            "/api/v1/assignments",
            new
            {
                weekId = Uri.UnescapeDataString(weekId),
                departmentId = IdentitySeedData.DeptPvhkDiId,
                slotId,
                employeeId = IdentitySeedData.EmpSupPvhkDiId,
            });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);

        var after = await client.GetAsync($"/api/v1/plans/{weekId}");
        after.EnsureSuccessStatusCode();
        var planAfter = await after.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("inProgress", planAfter.GetProperty("status").GetString());

        var assignBody = await assign.Content.ReadFromJsonAsync<JsonElement>();
        var assignmentId = assignBody.GetProperty("assignmentId").GetGuid();

        var boardResp = await client.GetAsync(
            $"/api/v1/supboard?weekId={weekId}&dayIdx={todayIdx}&departmentCode=PVHK_DI");
        boardResp.EnsureSuccessStatusCode();
        var boardJson = await boardResp.Content.ReadFromJsonAsync<JsonElement>();
        var slotFlights = boardJson.GetProperty("slots").EnumerateArray()
            .First(s => s.GetProperty("id").GetGuid() == slotId)
            .GetProperty("flightNos");
        var flightNo = slotFlights.GetArrayLength() > 0
            ? slotFlights[0].GetString()!
            : boardJson.GetProperty("flights")[0].GetProperty("flightNo").GetString()!;

        var link = await client.PutAsJsonAsync(
            $"/api/v1/assignments/{assignmentId}/flights",
            new { weekId = Uri.UnescapeDataString(weekId), flightNos = new[] { flightNo } });
        Assert.Equal(HttpStatusCode.OK, link.StatusCode);
        var linked = await link.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(flightNo, linked.GetProperty("flightNos").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task Staff_can_check_in_and_check_out()
    {
        var staff = await CreateClientAsync("AGS0138");
        var checkIn = await staff.PostAsJsonAsync(
            "/api/v1/attendance/check-in",
            new { lat = 11.998, lng = 109.219, inZone = true, geoSimulated = true });
        if (checkIn.StatusCode == HttpStatusCode.Conflict)
        {
            var checkOutFirst = await staff.PostAsJsonAsync(
                "/api/v1/attendance/check-out",
                new { earlyNote = (string?)null });
            checkOutFirst.EnsureSuccessStatusCode();
            checkIn = await staff.PostAsJsonAsync(
                "/api/v1/attendance/check-in",
                new { lat = 11.998, lng = 109.219, inZone = true, geoSimulated = true });
        }

        Assert.Equal(HttpStatusCode.OK, checkIn.StatusCode);

        var checkOut = await staff.PostAsJsonAsync(
            "/api/v1/attendance/check-out",
            new { earlyNote = "Ra ca sớm pitch" });
        Assert.Equal(HttpStatusCode.OK, checkOut.StatusCode);

        var body = await checkOut.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(body.GetProperty("checkOutUtc").GetString());
    }

    [Fact]
    public async Task Staff_sees_notifications_after_plan_publish()
    {
        var sup = await CreateClientAsync("AGS0184");
        var weekId = await GetAnyWeekIdAsync(sup);
        var gen = await sup.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        gen.EnsureSuccessStatusCode();
        var pub = await sup.PostAsync($"/api/v1/plans/{weekId}/publish", null);
        pub.EnsureSuccessStatusCode();

        var staff = await CreateClientAsync("AGS0138");
        var notif = await staff.GetAsync("/api/v1/notifications/me?page=0&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, notif.StatusCode);

        var body = await notif.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("items").GetArrayLength() > 0);
    }

    private static async Task<string> GetAnyWeekIdAsync(HttpClient client)
    {
        var listResponse = await client.GetAsync("/api/v1/flights?page=1&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var listJson = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        return Uri.EscapeDataString(
            listJson.GetProperty("items")[0].GetProperty("weekId").GetString()!);
    }

    private static string FindReferenceFlightPlanExcel()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "reference-data", "Kế_hoạch_bay.xlsx");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(dir);
            if (parent is null)
            {
                break;
            }

            dir = parent.FullName;
        }

        throw new FileNotFoundException("reference-data/Kế_hoạch_bay.xlsx not found from test output.");
    }

    private async Task<HttpClient> CreateClientAsync(string login)
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
