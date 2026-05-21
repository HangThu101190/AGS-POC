using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class WeeklyPlanRepository : IWeeklyPlanRepository
{
    private readonly SmartShiftDbContext _db;

    public WeeklyPlanRepository(SmartShiftDbContext db) => _db = db;

    public Task<WeeklyPlan?> GetByWeekIdAsync(string weekId, CancellationToken cancellationToken = default) =>
        _db.WeeklyPlans.AsNoTracking().FirstOrDefaultAsync(p => p.WeekId == weekId, cancellationToken);

    public Task<WeeklyPlan?> GetWithSlotsAsync(string weekId, CancellationToken cancellationToken = default) =>
        _db.WeeklyPlans
            .AsNoTracking()
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.WeekId == weekId, cancellationToken);

    public Task<WeeklyPlan?> GetWithSlotsForUpdateAsync(string weekId, CancellationToken cancellationToken = default) =>
        _db.WeeklyPlans
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.WeekId == weekId, cancellationToken);

    public async Task AddAsync(WeeklyPlan plan, CancellationToken cancellationToken = default)
    {
        await _db.WeeklyPlans.AddAsync(plan, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceFutureSlotsAsync(
        string weekId,
        IReadOnlyList<ShiftSlot> newSlots,
        int todayIdx,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var plan = await _db.WeeklyPlans
            .Include(p => p.Slots)
            .FirstAsync(p => p.WeekId == weekId, cancellationToken);

        var remove = plan.Slots.Where(s => s.DayIdx >= todayIdx).ToList();
        if (remove.Count > 0)
        {
            _db.ShiftSlots.RemoveRange(remove);
            plan.Slots.RemoveAll(s => s.DayIdx >= todayIdx);
        }

        if (newSlots.Count > 0)
        {
            await _db.ShiftSlots.AddRangeAsync(newSlots, cancellationToken);
            plan.Slots.AddRange(newSlots);
        }

        plan.Touch(utcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
