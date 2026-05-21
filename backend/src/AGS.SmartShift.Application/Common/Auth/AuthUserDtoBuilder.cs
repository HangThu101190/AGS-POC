using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Common.Auth;

public sealed class AuthUserDtoBuilder
{
    private readonly IDepartmentRepository _departments;
    private readonly IPermissionCatalogService _permissions;

    public AuthUserDtoBuilder(IDepartmentRepository departments, IPermissionCatalogService permissions)
    {
        _departments = departments;
        _permissions = permissions;
    }

    public async Task<AuthUserDto?> TryBuildAsync(UserAccount account, CancellationToken cancellationToken)
    {
        if (account.Employee is null)
        {
            return null;
        }

        var department = await _departments.GetByIdAsync(account.Employee.DepartmentId, cancellationToken);
        if (department is null)
        {
            return null;
        }

        var role = AuthUserMapper.ToRoleString(account.Employee.Role);
        var grants = await _permissions.GetPermissionsForRoleAsync(role, cancellationToken);
        return AuthUserMapper.ToDto(account, account.Employee, department, grants);
    }
}
