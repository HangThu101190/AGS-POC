using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Application.Common.Identity;

public static class IdentityDtoMapper
{
    public static DepartmentDto ToDepartmentDto(Department department) =>
        new()
        {
            Id = department.Id,
            SiteId = department.SiteId,
            Code = department.Code,
            Name = department.Name,
            IsActive = department.IsActive,
            AllowedRoles = department.AllowedRoles.ToList(),
            CreatedAt = department.CreatedAtUtc,
            UpdatedAt = department.UpdatedAtUtc,
        };

    public static EmployeeDto ToEmployeeDto(
        Employee employee,
        IReadOnlyDictionary<Guid, bool>? userActiveByEmployeeId = null)
    {
        var hasUser = userActiveByEmployeeId?.ContainsKey(employee.Id) == true;
        return new EmployeeDto
        {
            Id = employee.Id,
            DepartmentId = employee.DepartmentId,
            Code = employee.Code,
            Name = employee.Name,
            Role = employee.Role.ToString().ToLowerInvariant(),
            ManagerId = employee.ManagerId,
            IsActive = employee.IsActive,
            HasUser = hasUser,
            UserIsActive = hasUser ? userActiveByEmployeeId![employee.Id] : null,
            CreatedAt = employee.CreatedAtUtc,
            UpdatedAt = employee.UpdatedAtUtc,
        };
    }
}
