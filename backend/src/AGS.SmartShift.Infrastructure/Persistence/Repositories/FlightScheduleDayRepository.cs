using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class FlightScheduleDayRepository : IFlightScheduleDayRepository
{
    private readonly SmartShiftDbContext _db;

    public FlightScheduleDayRepository(SmartShiftDbContext db) => _db = db;

    public Task<FlightScheduleDay?> GetAsync(string weekId, int dayIdx, CancellationToken cancellationToken = default) =>
        _db.FlightScheduleDays.AsNoTracking()
            .FirstOrDefaultAsync(d => d.WeekId == weekId && d.DayIdx == dayIdx, cancellationToken);

    public async Task UpsertImportAsync(FlightScheduleDay day, CancellationToken cancellationToken = default)
    {
        var existing = await _db.FlightScheduleDays
            .FirstOrDefaultAsync(d => d.WeekId == day.WeekId && d.DayIdx == day.DayIdx, cancellationToken);

        if (existing is null)
        {
            await _db.FlightScheduleDays.AddAsync(day, cancellationToken);
        }
        else
        {
            existing.UpdateFromImport(day.SourceDayLabel, day.SheetRemark, DateTime.UtcNow);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
