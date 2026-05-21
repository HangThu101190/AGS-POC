using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class FlightImportJobRepository : IFlightImportJobRepository
{
    private readonly SmartShiftDbContext _db;

    public FlightImportJobRepository(SmartShiftDbContext db) => _db = db;

    public Task AddAsync(FlightImportJob job, CancellationToken cancellationToken = default)
    {
        _db.FlightImportJobs.Add(job);
        return Task.CompletedTask;
    }

    public Task<FlightImportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FlightImportJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
