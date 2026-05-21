using System.Net;
using System.Net.Http.Headers;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class AuthorizationTests
{
    private readonly SmartShiftApiFactory _factory;

    public AuthorizationTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task TC_AUTH_001_Staff_cannot_access_monitoring_snapshot()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/monitoring/snapshot");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TC_AUTH_002_Sup_cannot_assign_outside_own_department()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0184");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            departmentId = IdentitySeedData.DeptRampId,
            slotId = Guid.NewGuid(),
            employeeId = IdentitySeedData.EmpRampStaffId,
        };

        var response = await client.PostAsJsonAsync("/api/v1/assignments", body);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TC_AUTH_003_Staff_cannot_read_other_staff_attendance()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(
            $"/api/v1/attendance/{IdentitySeedData.EmpRampStaffId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
