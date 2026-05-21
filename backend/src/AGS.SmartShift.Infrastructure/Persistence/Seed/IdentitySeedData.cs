using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class IdentitySeedData
{
    public static readonly Guid CxrSiteId = Guid.Parse("11111111-1111-1111-1111-111111111101");

    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Sites.AnyAsync(cancellationToken))
        {
            await EnsureDemoEmployeesAsync(db, cancellationToken);
            await DepartmentRolesSeedData.SeedAsync(db, cancellationToken);
            return;
        }

        var seedTime = DateTime.UtcNow;

        var site = Site.Create("CXR", "AGS Cam Ranh", "Asia/Ho_Chi_Minh", seedTime);
        SetId(site, CxrSiteId);

        var departments = new[]
        {
            WithId(Department.Create(CxrSiteId, "PVHK_DI", "PVHK Đi", seedTime, DepartmentRolesDefaults.ForCode("PVHK_DI")), DeptPvhkDiId),
            WithId(Department.Create(CxrSiteId, "PVHK_DEN", "PVHK Đến", seedTime, DepartmentRolesDefaults.ForCode("PVHK_DEN")), DeptPvhkDenId),
            WithId(Department.Create(CxrSiteId, "RAMP", "Ramp", seedTime, DepartmentRolesDefaults.ForCode("RAMP")), DeptRampId),
            WithId(Department.Create(CxrSiteId, "BAGGAGE", "Baggage", seedTime, DepartmentRolesDefaults.ForCode("BAGGAGE")), DeptBaggageId),
            WithId(Department.Create(CxrSiteId, "HCNS", "HCNS", seedTime, DepartmentRolesDefaults.ForCode("HCNS")), DeptHcnsId),
        };

        var employees = new[]
        {
            WithId(Employee.Create(DeptHcnsId, "AGS0827", "Đặng Thị Lương", UserRole.Hr, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333301")),
            WithId(Employee.Create(DeptPvhkDiId, "AGS0184", "Đào Thúy Ngân", UserRole.Sup, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333302")),
            WithId(Employee.Create(DeptPvhkDenId, "AGS0138", "Phạm Thị Mai Anh", UserRole.Staff, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333303")),
            WithId(Employee.Create(DeptRampId, "AGS0101", "Nguyễn Văn A", UserRole.Staff, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333304")),
            WithId(Employee.Create(DeptBaggageId, "AGS0102", "Trần Thị B", UserRole.Staff, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333305")),
            WithId(Employee.Create(DeptHcnsId, "AGS0901", "Lê Minh Trực ban", UserRole.Tbdh, seedTime),
                Guid.Parse("33333333-3333-3333-3333-333333333306")),
            WithId(Employee.Create(DeptPvhkDiId, "AGS0139", "Nguyễn Văn Check-in Đi", UserRole.Staff, seedTime),
                EmpPvhkDiStaffId),
        };

        var att1 = AttendanceRecord.Create(
            Guid.Parse("44444444-4444-4444-4444-444444444401"),
            Guid.Parse("33333333-3333-3333-3333-333333333303"),
            seedTime.AddHours(-2),
            seedTime);
        att1.ApplyCheckIn(11.99825, 109.21925, inZone: true, geoNote: null, geoSimulated: false, seedTime);

        var att2 = AttendanceRecord.Create(
            Guid.Parse("44444444-4444-4444-4444-444444444402"),
            Guid.Parse("33333333-3333-3333-3333-333333333304"),
            seedTime.AddHours(-1),
            seedTime);
        att2.ApplyCheckIn(11.99835, 109.21945, inZone: true, geoNote: null, geoSimulated: false, seedTime);

        var attendance = new[] { att1, att2 };

        db.Sites.Add(site);
        db.Departments.AddRange(departments);
        db.Employees.AddRange(employees);
        db.AttendanceRecords.AddRange(attendance);
        await db.SaveChangesAsync(cancellationToken);
    }

    public static readonly Guid EmpPvhkDenStaffId = Guid.Parse("33333333-3333-3333-3333-333333333303");
    public static readonly Guid EmpRampStaffId = Guid.Parse("33333333-3333-3333-3333-333333333304");
    public static readonly Guid EmpSupPvhkDiId = Guid.Parse("33333333-3333-3333-3333-333333333302");
    public static readonly Guid EmpPvhkDiStaffId = Guid.Parse("33333333-3333-3333-3333-333333333307");
    public static readonly Guid EmpTbdhId = Guid.Parse("33333333-3333-3333-3333-333333333306");
    public static readonly Guid EmpShiftLeaderId = Guid.Parse("33333333-3333-3333-3333-333333333308");

    public static readonly Guid DeptPvhkDiId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid DeptPvhkDenId = Guid.Parse("22222222-2222-2222-2222-222222222202");
    public static readonly Guid DeptRampId = Guid.Parse("22222222-2222-2222-2222-222222222203");
    public static readonly Guid DeptBaggageId = Guid.Parse("22222222-2222-2222-2222-222222222204");
    public static readonly Guid DeptHcnsId = Guid.Parse("22222222-2222-2222-2222-222222222205");

    private static void SetId<T>(Entity<T> entity, T id) where T : struct, IEquatable<T>
    {
        typeof(Entity<T>).GetProperty(nameof(Entity<T>.Id))!.SetValue(entity, id);
    }

    private static TEntity WithId<TEntity, TId>(TEntity entity, TId id)
        where TEntity : Entity<TId>
        where TId : struct, IEquatable<TId>
    {
        SetId(entity, id);
        return entity;
    }

    /// <summary>Adds demo employees introduced after the first DB seed (idempotent by employee code).</summary>
    private static async Task EnsureDemoEmployeesAsync(
        SmartShiftDbContext db,
        CancellationToken cancellationToken)
    {
        var existingCodes = await db.Employees
            .Select(e => e.Code)
            .ToListAsync(cancellationToken);
        var seedTime = DateTime.UtcNow;
        var toAdd = new List<Employee>();

        foreach (var (id, deptId, code, name, role) in DemoEmployees())
        {
            if (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            toAdd.Add(WithId(Employee.Create(deptId, code, name, role, seedTime), id));
        }

        if (toAdd.Count == 0)
        {
            return;
        }

        db.Employees.AddRange(toAdd);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<(Guid Id, Guid DeptId, string Code, string Name, UserRole Role)> DemoEmployees()
    {
        yield return (
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            DeptHcnsId,
            "AGS0827",
            "Đặng Thị Lương",
            UserRole.Hr);
        yield return (
            Guid.Parse("33333333-3333-3333-3333-333333333302"),
            DeptPvhkDiId,
            "AGS0184",
            "Đào Thúy Ngân",
            UserRole.Sup);
        yield return (
            Guid.Parse("33333333-3333-3333-3333-333333333303"),
            DeptPvhkDenId,
            "AGS0138",
            "Phạm Thị Mai Anh",
            UserRole.Staff);
        yield return (
            Guid.Parse("33333333-3333-3333-3333-333333333304"),
            DeptRampId,
            "AGS0101",
            "Nguyễn Văn A",
            UserRole.Staff);
        yield return (
            Guid.Parse("33333333-3333-3333-3333-333333333305"),
            DeptBaggageId,
            "AGS0102",
            "Trần Thị B",
            UserRole.Staff);
        yield return (
            EmpTbdhId,
            DeptHcnsId,
            "AGS0901",
            "Lê Minh Trực ban",
            UserRole.Tbdh);
        yield return (
            EmpPvhkDiStaffId,
            DeptPvhkDiId,
            "AGS0139",
            "Nguyễn Văn Check-in Đi",
            UserRole.Staff);
        yield return (
            EmpShiftLeaderId,
            DeptPvhkDiId,
            "AGS0185",
            "Trần Văn Trưởng Ca",
            UserRole.ShiftLeader);
    }
}
