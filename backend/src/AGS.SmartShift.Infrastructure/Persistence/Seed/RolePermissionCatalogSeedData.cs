using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class RolePermissionCatalogSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.SystemRoles.AnyAsync(cancellationToken))
        {
            return;
        }

        var roles = new[]
        {
            SystemRole.Create("tbdh", "Trực ban điều hành", "Duty manager", 1, "Lịch bay, import, delay"),
            SystemRole.Create("hr", "HCNS", "HR", 2, "Kế hoạch, đối soát, master data"),
            SystemRole.Create("sup", "Giám sát ca", "Shift supervisor", 3, "Phân ca, gán chuyến"),
            SystemRole.Create("staff", "Nhân viên", "Staff", 4, "Check-in, xem lịch"),
        };

        var permissions = new[]
        {
            SystemPermission.Create("flights.import", "flights", "Import lịch bay", "Import flight schedule"),
            SystemPermission.Create("flights.delay", "flights", "Cập nhật delay", "Update flight delay"),
            SystemPermission.Create("flights.crud", "flights", "Quản lý chuyến bay", "Flight CRUD"),
            SystemPermission.Create("plan.publish", "bangPhanCa", "Bảng phân ca · phát hành", "Shift board · publish"),
            SystemPermission.Create("assignments.mutate", "phanCongSlot", "Phân công slot", "Slot assignments"),
            SystemPermission.Create("attendance.checkin", "attendance", "Check-in/out", "Check-in and check-out"),
            SystemPermission.Create("monitoring.view", "monitoring", "Giám sát vị trí", "Location monitoring"),
            SystemPermission.Create("reconcile.export", "reconcile", "Đối soát / xuất công", "Reconcile and export"),
            SystemPermission.Create("config.workzone", "config", "Cấu hình geofence", "Work zone config"),
            SystemPermission.Create("master.employees", "people", "Quản lý nhân viên", "Employee master data"),
            SystemPermission.Create("master.departments", "people", "Quản lý phòng ban", "Department master data"),
            SystemPermission.Create("audit.view", "audit", "Xem audit", "View audit log"),
        };

        var grants = new List<SystemRolePermission>
        {
            // tbdh
            SystemRolePermission.Create("tbdh", "flights.import"),
            SystemRolePermission.Create("tbdh", "flights.delay"),
            SystemRolePermission.Create("tbdh", "flights.crud"),
            // hr
            SystemRolePermission.Create("hr", "flights.import"),
            SystemRolePermission.Create("hr", "flights.delay"),
            SystemRolePermission.Create("hr", "plan.publish"),
            SystemRolePermission.Create("hr", "monitoring.view"),
            SystemRolePermission.Create("hr", "reconcile.export"),
            SystemRolePermission.Create("hr", "config.workzone"),
            SystemRolePermission.Create("hr", "master.employees"),
            SystemRolePermission.Create("hr", "master.departments"),
            SystemRolePermission.Create("hr", "audit.view"),
            // sup
            SystemRolePermission.Create("sup", "assignments.mutate"),
            SystemRolePermission.Create("sup", "plan.publish"),
            // staff
            SystemRolePermission.Create("staff", "attendance.checkin"),
        };

        db.SystemRoles.AddRange(roles);
        db.SystemPermissions.AddRange(permissions);
        db.SystemRolePermissions.AddRange(grants);
        await db.SaveChangesAsync(cancellationToken);
    }
}
