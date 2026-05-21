using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IFlightScheduleDayRepository
{
    Task<FlightScheduleDay?> GetAsync(string weekId, int dayIdx, CancellationToken cancellationToken = default);

    Task UpsertImportAsync(FlightScheduleDay day, CancellationToken cancellationToken = default);
}
