using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Application.Common.Identity;

public static class DepartmentRoleGuard
{
    public static void EnsureRoleAllowed(Department department, string roleCode)
    {
        if (!department.IsActive)
        {
            throw new DomainException("department_inactive", "Phòng ban đã ngừng hoạt động.");
        }

        if (!department.AllowsRole(roleCode))
        {
            throw new DomainException(
                "role_not_allowed_in_department",
                $"Vai trò '{roleCode}' không được phép trong phòng ban {department.Code}.");
        }
    }
}
