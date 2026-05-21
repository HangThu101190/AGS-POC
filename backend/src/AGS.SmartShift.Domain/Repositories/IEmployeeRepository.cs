using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Domain.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Employee?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<PagedResult<Employee>> ListAsync(
        TableListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, int>> CountActiveStaffByDepartmentCodeAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
