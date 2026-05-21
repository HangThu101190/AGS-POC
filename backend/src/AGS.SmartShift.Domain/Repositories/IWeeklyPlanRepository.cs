using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IWeeklyPlanRepository
{
    Task<WeeklyPlan?> GetByWeekIdAsync(string weekId, CancellationToken cancellationToken = default);

    Task<WeeklyPlan?> GetWithSlotsAsync(string weekId, CancellationToken cancellationToken = default);

    Task<WeeklyPlan?> GetWithSlotsForUpdateAsync(string weekId, CancellationToken cancellationToken = default);

    Task AddAsync(WeeklyPlan plan, CancellationToken cancellationToken = default);

    Task ReplaceFutureSlotsAsync(
        string weekId,
        IReadOnlyList<ShiftSlot> newSlots,
        int todayIdx,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
