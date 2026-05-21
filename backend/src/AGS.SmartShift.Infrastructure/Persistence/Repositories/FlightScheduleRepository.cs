using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class FlightScheduleRepository : IFlightScheduleRepository
{
    private readonly SmartShiftDbContext _db;

    public FlightScheduleRepository(SmartShiftDbContext db) => _db = db;

    public Task<FlightSchedule?> GetByWeekIdAsync(string weekId, CancellationToken cancellationToken = default) =>
        _db.FlightSchedules.FirstOrDefaultAsync(s => s.WeekId == weekId, cancellationToken);

    public async Task<FlightSchedule> GetOrCreateForUpdateAsync(
        Guid siteId,
        string weekId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.FlightSchedules.FirstOrDefaultAsync(s => s.WeekId == weekId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var schedule = FlightSchedule.Create(siteId, weekId, utcNow);
        await _db.FlightSchedules.AddAsync(schedule, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
