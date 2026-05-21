using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Common.Auth;

public static class AuthUserMapper
{
    public static AuthUserDto ToDto(
        UserAccount account,
        Employee employee,
        Department department,
        IReadOnlyList<string> permissions) =>
        new(
            employee.Id,
            employee.Code,
            employee.Name,
            ToRoleString(employee.Role),
            employee.DepartmentId,
            account.Id,
            account.LoginName,
            department.Code,
            department.Name,
            account.PreferredLanguage,
            account.MustChangePassword,
            permissions);

    public static string ToRoleString(UserRole role) =>
        role switch
        {
            UserRole.Hr => SmartShiftRoles.Hr,
            UserRole.Sup => SmartShiftRoles.Sup,
            UserRole.Staff => SmartShiftRoles.Staff,
            UserRole.Tbdh => SmartShiftRoles.Tbdh,
            UserRole.ShiftLeader => SmartShiftRoles.ShiftLeader,
            _ => SmartShiftRoles.Staff,
        };
}
