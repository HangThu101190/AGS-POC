namespace AGS.SmartShift.Application.Common.Authorization;

public interface IResourceAccessService
{
    bool CanAccessDepartment(Guid departmentId);

    Task<bool> CanAccessEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
