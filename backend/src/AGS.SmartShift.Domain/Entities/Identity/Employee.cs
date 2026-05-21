using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class Employee : AuditableEntity<Guid>
{
    public Guid DepartmentId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? ManagerId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public OperationalSegment? DefaultSegment { get; private set; }
    public string? PreferredShiftCode { get; private set; }
    public decimal MaxWeeklyHours { get; private set; } = 48m;

    private Employee()
    {
    }

    public static Employee Create(
        Guid departmentId,
        string code,
        string name,
        UserRole role,
        DateTime utcNow,
        Guid? managerId = null)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Role = role,
            ManagerId = managerId,
        };
        employee.MarkCreated(utcNow);
        return employee;
    }

    public void UpdateProfile(
        Guid departmentId,
        string name,
        UserRole role,
        DateTime utcNow,
        Guid? managerId = null)
    {
        DepartmentId = departmentId;
        Name = name.Trim();
        Role = role;
        ManagerId = managerId;
        MarkUpdated(utcNow);
    }

    public void SetActive(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        MarkUpdated(utcNow);
    }

    public void UpdateStaffingPreferences(
        OperationalSegment? defaultSegment,
        string? preferredShiftCode,
        decimal maxWeeklyHours,
        DateTime utcNow)
    {
        DefaultSegment = defaultSegment;
        PreferredShiftCode = string.IsNullOrWhiteSpace(preferredShiftCode)
            ? null
            : preferredShiftCode.Trim().ToUpperInvariant();
        MaxWeeklyHours = maxWeeklyHours > 0 ? maxWeeklyHours : 48m;
        MarkUpdated(utcNow);
    }
}
