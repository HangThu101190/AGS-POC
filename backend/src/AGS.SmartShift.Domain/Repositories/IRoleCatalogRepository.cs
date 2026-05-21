using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Domain.Repositories;

public interface IRoleCatalogRepository
{
    Task<IReadOnlyList<SystemRole>> ListRolesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemPermission>> ListPermissionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemRolePermission>> ListRolePermissionsAsync(CancellationToken cancellationToken = default);

    Task ReplaceAllRolePermissionsAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> grantsByRole,
        CancellationToken cancellationToken = default);
}
