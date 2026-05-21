using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class WorkZoneRepository : IWorkZoneRepository
{
    private readonly SmartShiftDbContext _db;

    public WorkZoneRepository(SmartShiftDbContext db) => _db = db;

    public Task<WorkZone?> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default) =>
        _db.WorkZones
            .AsNoTracking()
            .Where(z => z.SiteId == siteId && z.IsActive)
            .OrderByDescending(z => z.Version)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<WorkZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.WorkZones.FirstOrDefaultAsync(z => z.Id == id, cancellationToken);

    public Task AddAsync(WorkZone zone, CancellationToken cancellationToken = default)
    {
        _db.WorkZones.Add(zone);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
