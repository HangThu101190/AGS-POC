using System.Globalization;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using AGS.SmartShift.Infrastructure.Persistence.Seed;

namespace AGS.SmartShift.Infrastructure.Services;

public sealed class PlanningWeekService : IPlanningWeekService
{
    private readonly IWeeklyPlanRepository _plans;
    private readonly IDateTimeProvider _clock;

    public PlanningWeekService(IWeeklyPlanRepository plans, IDateTimeProvider clock)
    {
        _plans = plans;
        _clock = clock;
    }

    public Task<WeeklyPlan> GetOrCreateCurrentWeekAsync(CancellationToken cancellationToken = default)
    {
        var (weekId, _, _, _) = WeekCalendar.BuildCurrentWeek(_clock.UtcNow.AddHours(7).Date);
        return GetOrCreateWeekAsync(weekId, cancellationToken);
    }

    public async Task<WeeklyPlan> GetOrCreateWeekAsync(
        string? weekId,
        CancellationToken cancellationToken = default)
    {
        var resolvedWeekId = string.IsNullOrWhiteSpace(weekId)
            ? WeekCalendar.BuildCurrentWeek(_clock.UtcNow.AddHours(7).Date).WeekId
            : weekId.Trim();

        var existing = await _plans.GetWithSlotsAsync(resolvedWeekId, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var referenceLocal = _clock.UtcNow.AddHours(7).Date;
        if (!WeekCalendar.TryParseWeekId(resolvedWeekId, out var isoYear, out var isoWeek))
        {
            (isoYear, isoWeek) = (ISOWeek.GetYear(referenceLocal), ISOWeek.GetWeekOfYear(referenceLocal));
        }

        var (_, weekYear, todayIdx, dates) = WeekCalendar.BuildWeek(isoYear, isoWeek, referenceLocal);

        var plan = WeeklyPlan.Create(
            IdentitySeedData.CxrSiteId,
            resolvedWeekId,
            weekYear,
            todayIdx,
            dates,
            _clock.UtcNow);
        await _plans.AddAsync(plan, cancellationToken);
        await _plans.SaveChangesAsync(cancellationToken);
        return plan;
    }
}
