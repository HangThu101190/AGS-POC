using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class SyncTests
{
    private readonly SmartShiftApiFactory _factory;

    public SyncTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Delay_creates_proposal_Sup_confirms_staff_sees_shift_changed()
    {
        var sup = await CreateClientAsync("AGS0184");
        var weekId = await GetAnyWeekIdAsync(sup);
        var decodedWeek = Uri.UnescapeDataString(weekId);

        await sup.PostAsync($"/api/v1/plans/{weekId}/slots/generate", null);
        await sup.PostAsync($"/api/v1/plans/{weekId}/publish", null);

        var plan = await (await sup.GetAsync($"/api/v1/plans/{weekId}"))
            .Content.ReadFromJsonAsync<JsonElement>();
        var todayIdx = plan!.GetProperty("todayIdx").GetInt32();

        Guid slotId = Guid.Empty;
        string[] beforeSegments = [];
        var deptCode = "PVHK_DI";
        var deptId = IdentitySeedData.DeptPvhkDiId;
        foreach (var slot in plan.GetProperty("slots").EnumerateArray())
        {
            if (slot.GetProperty("dayIdx").GetInt32() != todayIdx
                || slot.GetProperty("departmentCode").GetString() != deptCode)
            {
                continue;
            }

            slotId = slot.GetProperty("id").GetGuid();
            beforeSegments = slot.GetProperty("segments").EnumerateArray()
                .Select(s => s.GetString()!)
                .ToArray();
            break;
        }

        Assert.NotEqual(Guid.Empty, slotId);

        var assign = await sup.PostAsJsonAsync(
            "/api/v1/assignments",
            new
            {
                weekId = decodedWeek,
                departmentId = deptId,
                slotId,
                employeeId = IdentitySeedData.EmpPvhkDiStaffId,
            });
        assign.EnsureSuccessStatusCode();
        var assignmentId = (await assign.Content.ReadFromJsonAsync<JsonElement>())!
            .GetProperty("assignmentId")
            .GetGuid();

        var boardResp = await sup.GetAsync(
            $"/api/v1/supboard?weekId={weekId}&dayIdx={todayIdx}&departmentCode={deptCode}");
        boardResp.EnsureSuccessStatusCode();
        var board = await boardResp.Content.ReadFromJsonAsync<JsonElement>();
        var dayFlight = board!.GetProperty("flights").EnumerateArray().FirstOrDefault();
        Assert.True(dayFlight.ValueKind != JsonValueKind.Undefined);
        var flightNo = dayFlight.GetProperty("flightNo").GetString()!;
        var flightId = dayFlight.GetProperty("id").GetGuid();

        await sup.PutAsJsonAsync(
            $"/api/v1/assignments/{assignmentId}/flights",
            new { weekId = decodedWeek, flightNos = new[] { flightNo } });

        var tbdh = await CreateClientAsync("AGS0901");
        var delayResp = await tbdh.PatchAsJsonAsync(
            $"/api/v1/flights/{flightId}/delay",
            new { delayMinutes = 75 });
        delayResp.EnsureSuccessStatusCode();

        var listResp = await sup.GetAsync($"/api/v1/sync-proposals?weekId={weekId}&pendingOnly=true");
        listResp.EnsureSuccessStatusCode();
        var proposals = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(proposals!.GetArrayLength() > 0);
        var proposalId = proposals[0].GetProperty("id").GetGuid();
        var proposed = proposals[0].GetProperty("affected")[0].GetProperty("proposedSegments")
            .EnumerateArray()
            .Select(s => s.GetString()!)
            .ToArray();
        Assert.NotEqual(beforeSegments, proposed);

        var confirm = await sup.PostAsync($"/api/v1/sync-proposals/{proposalId}/confirm", null);
        confirm.EnsureSuccessStatusCode();

        var planAfter = await (await sup.GetAsync($"/api/v1/plans/{weekId}"))
            .Content.ReadFromJsonAsync<JsonElement>();
        var slotAfter = planAfter!.GetProperty("slots").EnumerateArray()
            .First(s => s.GetProperty("id").GetGuid() == slotId);
        var afterSegments = slotAfter.GetProperty("segments").EnumerateArray()
            .Select(s => s.GetString()!)
            .ToArray();
        Assert.Equal(proposed, afterSegments);

        var staff = await CreateClientAsync("AGS0139");
        var shiftResp = await staff.GetAsync($"/api/v1/attendance/my-shift?weekId={weekId}");
        shiftResp.EnsureSuccessStatusCode();
        var shift = await shiftResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(shift!.GetProperty("shiftChanged").GetBoolean());
        Assert.Equal(proposed, shift.GetProperty("segments").EnumerateArray().Select(s => s.GetString()).ToArray());
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
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
