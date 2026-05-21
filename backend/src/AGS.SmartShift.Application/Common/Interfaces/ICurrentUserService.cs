namespace AGS.SmartShift.Application.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid? UserAccountId { get; }

    Guid? EmployeeId { get; }

    Guid? DepartmentId { get; }

    /// <summary>Lowercase role: hr, sup, staff.</summary>
    string? Role { get; }

    bool IsInRole(string role);
}
