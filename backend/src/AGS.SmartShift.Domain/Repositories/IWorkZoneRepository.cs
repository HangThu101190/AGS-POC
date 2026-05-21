using AGS.SmartShift.Domain.Entities.Attendance;

namespace AGS.SmartShift.Domain.Repositories;

public interface IWorkZoneRepository
{
    Task<WorkZone?> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<WorkZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(WorkZone zone, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
