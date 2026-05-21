namespace AGS.SmartShift.Application.Contracts.Auth;

public sealed record AuthUserDto(
    Guid Id,
    string Code,
    string Name,
    string Role,
    Guid DepartmentId,
    Guid UserId,
    string LoginName,
    string DepartmentCode,
    string DepartmentName,
    string PreferredLanguage,
    bool MustChangePassword,
    IReadOnlyList<string> Permissions);
