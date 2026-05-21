using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IPlanningWeekService
{
    Task<WeeklyPlan> GetOrCreateCurrentWeekAsync(CancellationToken cancellationToken = default);

    Task<WeeklyPlan> GetOrCreateWeekAsync(string? weekId, CancellationToken cancellationToken = default);
}
