namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class EmployeeUpsertDto
{
    public required Guid DepartmentId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public Guid? ManagerId { get; init; }
    public string? InitialPassword { get; init; }
}
