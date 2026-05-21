using AGS.SmartShift.Application.Contracts.Common;

namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class EmployeeDto : AuditableResourceDto
{
    public required Guid DepartmentId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public Guid? ManagerId { get; init; }
    public bool IsActive { get; init; }
    public bool HasUser { get; init; }
    public bool? UserIsActive { get; init; }
}
