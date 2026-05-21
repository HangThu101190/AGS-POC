namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class EmployeeUserDto
{
    public required Guid UserId { get; init; }
    public required string LoginName { get; init; }
    public required bool IsActive { get; init; }
    public required bool HasUser { get; init; }
    public string? TemporaryPassword { get; init; }
}

public sealed class ResetPasswordResultDto
{
    public required string TemporaryPassword { get; init; }
}
