using AGS.SmartShift.Application.Common.Identity;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>Backfills <c>allowed_roles_json</c> on existing departments.</summary>
public static class DepartmentRolesSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        var departments = await db.Departments.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var changed = false;

        foreach (var dept in departments)
        {
            if (dept.AllowedRoles.Count == 0)
            {
                dept.SetAllowedRoles(DepartmentRolesDefaults.ForCode(dept.Code).ToList(), now);
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
