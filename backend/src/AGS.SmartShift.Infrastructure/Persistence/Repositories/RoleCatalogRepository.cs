using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class RoleCatalogRepository : IRoleCatalogRepository
{
    private readonly SmartShiftDbContext _db;

    public RoleCatalogRepository(SmartShiftDbContext db) => _db = db;

    public async Task<IReadOnlyList<SystemRole>> ListRolesAsync(CancellationToken cancellationToken = default) =>
        await _db.SystemRoles
            .AsNoTracking()
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SystemPermission>> ListPermissionsAsync(CancellationToken cancellationToken = default) =>
        await _db.SystemPermissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SystemRolePermission>> ListRolePermissionsAsync(CancellationToken cancellationToken = default) =>
        await _db.SystemRolePermissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task ReplaceAllRolePermissionsAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> grantsByRole,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.SystemRolePermissions.ToListAsync(cancellationToken);
        _db.SystemRolePermissions.RemoveRange(existing);

        foreach (var (roleCode, codes) in grantsByRole)
        {
            foreach (var code in codes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                _db.SystemRolePermissions.Add(SystemRolePermission.Create(roleCode, code));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
