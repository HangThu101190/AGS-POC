using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Domain.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Department?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(Guid siteId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<PagedResult<Department>> ListAsync(
        TableListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
