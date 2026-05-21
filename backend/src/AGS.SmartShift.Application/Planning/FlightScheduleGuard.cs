using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Planning;

public static class FlightScheduleGuard
{
    public static async Task EnsureMutableAsync(
        IFlightScheduleRepository schedules,
        string weekId,
        string actionLabel,
        CancellationToken cancellationToken)
    {
        var schedule = await schedules.GetByWeekIdAsync(weekId, cancellationToken);
        schedule?.EnsureMutable(actionLabel);
    }
}
