using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IFlightScheduleRepository
{
    Task<FlightSchedule?> GetByWeekIdAsync(string weekId, CancellationToken cancellationToken = default);

    Task<FlightSchedule> GetOrCreateForUpdateAsync(
        Guid siteId,
        string weekId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
