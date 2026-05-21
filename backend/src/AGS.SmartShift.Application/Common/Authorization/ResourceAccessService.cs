using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Common.Authorization;

public sealed class ResourceAccessService : IResourceAccessService
{
    private readonly ICurrentUserService _user;
    private readonly IEmployeeRepository _employees;

    public ResourceAccessService(ICurrentUserService user, IEmployeeRepository employees)
    {
        _user = user;
        _employees = employees;
    }

    public bool CanAccessDepartment(Guid departmentId)
    {
        if (!_user.IsAuthenticated)
        {
            return false;
        }

        if (_user.IsInRole(SmartShiftRoles.Hr) || _user.IsInRole(SmartShiftRoles.Tbdh))
        {
            return true;
        }

        return _user.DepartmentId == departmentId;
    }

    public async Task<bool> CanAccessEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!_user.IsAuthenticated)
        {
            return false;
        }

        if (_user.IsInRole(SmartShiftRoles.Hr))
        {
            return true;
        }

        if (_user.IsInRole(SmartShiftRoles.Staff))
        {
            return _user.EmployeeId == employeeId;
        }

        if (_user.DepartmentId is not Guid deptId)
        {
            return false;
        }

        var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
        return employee is not null && employee.DepartmentId == deptId;
    }
}
