using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Common.Identity;

public static class UserRoleParser
{
    public static UserRole Parse(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new DomainException("invalid_role", "Vai trò không hợp lệ.");
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "hr" => UserRole.Hr,
            "sup" => UserRole.Sup,
            "staff" => UserRole.Staff,
            "tbdh" => UserRole.Tbdh,
            "shift_leader" or "shiftleader" => UserRole.ShiftLeader,
            _ => throw new DomainException("invalid_role", "Vai trò không hợp lệ."),
        };
    }
}
