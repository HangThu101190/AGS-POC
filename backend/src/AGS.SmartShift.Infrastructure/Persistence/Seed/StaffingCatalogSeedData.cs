using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>Idempotent catalog updates for daily staffing (shift_leader role + permissions).</summary>
public static class StaffingCatalogSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync(db, cancellationToken);
        await EnsurePermissionModuleRenamesAsync(db, cancellationToken);
        await EnsurePermissionsAsync(db, cancellationToken);
    }

    /// <summary>Idempotent: planner/supboard → bangphanca/phancongslot (matches FE route keys).</summary>
    private static async Task EnsurePermissionModuleRenamesAsync(
        SmartShiftDbContext db,
        CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE system_permissions SET module = 'bangphanca'
            WHERE code = 'plan.publish' AND module = 'planner';
            UPDATE system_permissions SET module = 'phancongslot'
            WHERE code = 'assignments.mutate' AND module = 'supboard';
            """,
            cancellationToken);
    }

    private static async Task EnsureRoleAsync(SmartShiftDbContext db, CancellationToken cancellationToken)
    {
        if (await db.SystemRoles.AnyAsync(r => r.Code == "shift_leader", cancellationToken))
        {
            return;
        }

        db.SystemRoles.Add(SystemRole.Create(
            "shift_leader",
            "Trưởng ca",
            "Shift leader",
            5,
            "Phân công ngày PVHK, xác nhận lịch"));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsurePermissionsAsync(SmartShiftDbContext db, CancellationToken cancellationToken)
    {
        var codes = new[]
        {
            ("staffing.view", "staffing", "Xem phân công ngày", "View daily staffing"),
            ("staffing.tbdh.edit", "staffing", "TBĐH chỉnh định biên", "TBĐH edit manning"),
            ("staffing.assign", "staffing", "Trưởng ca gán NV", "Shift leader assign crew"),
            ("staffing.confirm", "staffing", "Xác nhận phân công", "Confirm daily staffing"),
            ("staffing.export", "staffing", "Xuất PVHK ngày", "Export PVHK daily"),
            ("config.manning_rules", "config", "Quy tắc định biên", "Manning rules config"),
            ("config.shift_attendance", "config", "Giờ chấm công theo ca", "Shift check-in policy"),
            ("leave.request", "leave", "Xin phép", "Leave request"),
            ("leave.approve", "leave", "Duyệt phép", "Approve leave"),
            ("leave.view", "leave", "Xem phép", "View leave"),
            ("planning.hub.subscribe", "planning", "Realtime planning", "Planning hub subscribe"),
        };

        var existing = await db.SystemPermissions
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        foreach (var (code, module, nameVi, nameEn) in codes)
        {
            if (existing.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            db.SystemPermissions.Add(SystemPermission.Create(code, module, nameVi, nameEn));
        }

        await db.SaveChangesAsync(cancellationToken);
        await EnsureGrantsAsync(db, cancellationToken);
    }

    private static async Task EnsureGrantsAsync(SmartShiftDbContext db, CancellationToken cancellationToken)
    {
        var grants = new List<(string Role, string Permission)>
        {
            ("shift_leader", "staffing.view"),
            ("shift_leader", "staffing.assign"),
            ("shift_leader", "staffing.confirm"),
            ("shift_leader", "staffing.export"),
            ("shift_leader", "leave.view"),
            ("shift_leader", "planning.hub.subscribe"),
            ("tbdh", "staffing.view"),
            ("tbdh", "staffing.tbdh.edit"),
            ("tbdh", "planning.hub.subscribe"),
            ("hr", "staffing.view"),
            ("hr", "staffing.tbdh.edit"),
            ("hr", "staffing.assign"),
            ("hr", "staffing.confirm"),
            ("hr", "staffing.export"),
            ("hr", "config.manning_rules"),
            ("hr", "config.shift_attendance"),
            ("hr", "leave.approve"),
            ("hr", "leave.view"),
            ("hr", "planning.hub.subscribe"),
            ("staff", "leave.request"),
        };

        var existing = await db.SystemRolePermissions
            .Select(g => new { g.RoleCode, g.PermissionCode })
            .ToListAsync(cancellationToken);

        foreach (var (role, permission) in grants)
        {
            if (existing.Any(g =>
                    g.RoleCode.Equals(role, StringComparison.OrdinalIgnoreCase) &&
                    g.PermissionCode.Equals(permission, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            db.SystemRolePermissions.Add(SystemRolePermission.Create(role, permission));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
