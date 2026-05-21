using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public sealed class IdentityAdminTests
{
    private readonly SmartShiftApiFactory _factory;

    public IdentityAdminTests(SmartShiftApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Patch_me_updates_preferred_language()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patch = await client.PatchAsJsonAsync("/api/v1/auth/me", new { preferredLanguage = "en" });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var me = await patch.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal("en", me?.PreferredLanguage);

        var loginAgain = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginAgain);
        var me2 = await client.GetFromJsonAsync<MeResponse>("/api/v1/auth/me");
        Assert.Equal("en", me2?.PreferredLanguage);
    }

    [Fact]
    public async Task Roles_catalog_returns_four_roles()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0827");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var catalog = await client.GetFromJsonAsync<RoleCatalogResponse>("/api/v1/roles/catalog");
        Assert.NotNull(catalog);
        Assert.Equal(4, catalog!.Roles.Count);
        Assert.Contains(catalog.Roles, r => r.Code == "hr");
    }

    [Fact]
    public async Task Hr_can_update_role_catalog_and_me_includes_permissions()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0827");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var catalog = await client.GetFromJsonAsync<RoleCatalogFullResponse>("/api/v1/roles/catalog");
        Assert.NotNull(catalog);

        var body = new Dictionary<string, List<string>>();
        foreach (var role in catalog!.Roles)
        {
            body[role.Code] = role.Permissions.ToList();
        }

        var put = await client.PutAsJsonAsync("/api/v1/roles/catalog/permissions", new { roles = body });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var me = await client.GetFromJsonAsync<MeWithPermissionsResponse>("/api/v1/auth/me");
        Assert.NotNull(me?.Permissions);
        Assert.Contains("master.employees", me!.Permissions);
    }

    [Fact]
    public async Task Staff_cannot_update_role_catalog()
    {
        var client = _factory.CreateClient();
        var token = await _factory.LoginAsync(client, "AGS0138");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var catalog = await client.GetFromJsonAsync<RoleCatalogFullResponse>("/api/v1/roles/catalog");
        var body = new Dictionary<string, List<string>>();
        foreach (var role in catalog!.Roles)
        {
            body[role.Code] = role.Permissions.ToList();
        }

        var put = await client.PutAsJsonAsync("/api/v1/roles/catalog/permissions", new { roles = body });
        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
    }

    private sealed class MeResponse
    {
        [JsonPropertyName("preferredLanguage")]
        public string PreferredLanguage { get; set; } = "";
    }

    private sealed class RoleCatalogResponse
    {
        [JsonPropertyName("roles")]
        public List<RoleItem> Roles { get; set; } = new();
    }

    private sealed class RoleItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
    }

    private sealed class RoleCatalogFullResponse
    {
        [JsonPropertyName("roles")]
        public List<RoleCatalogRoleItem> Roles { get; set; } = new();
    }

    private sealed class RoleCatalogRoleItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();
    }

    private sealed class MeWithPermissionsResponse
    {
        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();
    }
}
