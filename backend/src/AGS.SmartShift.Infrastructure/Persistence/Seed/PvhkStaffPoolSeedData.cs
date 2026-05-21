using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>
/// PVHK Đi staff pool for crew auto-assign demos (~400 active staff with qualifications).
/// Idempotent: tops up until <see cref="TargetStaffCount"/> staff in PVHK_DI.
/// </summary>
public static class PvhkStaffPoolSeedData
{
    public const int TargetStaffCount = 400;
    private const int BatchSize = 100;
    private const string CodePrefix = "PVHK";

    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        var deptId = IdentitySeedData.DeptPvhkDiId;
        var staffCount = await db.Employees.CountAsync(
            e => e.DepartmentId == deptId && e.Role == UserRole.Staff && e.IsActive,
            cancellationToken);
        if (staffCount >= TargetStaffCount)
        {
            return;
        }

        var existingCodes = await db.Employees
            .AsNoTracking()
            .Select(e => e.Code)
            .ToListAsync(cancellationToken);
        var codeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var needed = TargetStaffCount - staffCount;
        var created = 0;
        var seq = 1;

        while (created < needed && seq < 100_000)
        {
            var employees = new List<Employee>(BatchSize);
            var qualifications = new List<EmployeeQualification>(BatchSize * 5);

            while (employees.Count < BatchSize && created < needed)
            {
                string code;
                do
                {
                    code = $"{CodePrefix}{seq:D5}";
                    seq++;
                }
                while (!codeSet.Add(code));

                var employee = Employee.Create(
                    deptId,
                    code,
                    $"Nhân viên PVHK {code}",
                    UserRole.Staff,
                    now);
                employees.Add(employee);
                qualifications.AddRange(BuildDefaultQualifications(employee.Id, now));
                created++;
            }

            db.Employees.AddRange(employees);
            db.EmployeeQualifications.AddRange(qualifications);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static IEnumerable<EmployeeQualification> BuildDefaultQualifications(Guid employeeId, DateTime utcNow)
    {
        yield return EmployeeQualification.Create(employeeId, OperationalSegment.Qn, CrewRole.General, 2, utcNow);
        yield return EmployeeQualification.Create(employeeId, OperationalSegment.Qn, CrewRole.Counter, 2, utcNow);
        yield return EmployeeQualification.Create(employeeId, OperationalSegment.Qn, CrewRole.Gate, 2, utcNow);
        yield return EmployeeQualification.Create(employeeId, OperationalSegment.Qt, CrewRole.General, 2, utcNow);
        yield return EmployeeQualification.Create(employeeId, OperationalSegment.Qt, CrewRole.Sup, 2, utcNow);
    }
}
