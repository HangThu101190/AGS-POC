namespace AGS.SmartShift.Application.Common.Authorization;

public interface IPermissionCatalogService
{
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(string roleCode, CancellationToken cancellationToken = default);

    void InvalidateCache();
}
