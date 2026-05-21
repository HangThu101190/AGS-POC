using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed class ResetWeeklyPlanCommandHandler : IRequestHandler<ResetWeeklyPlanCommand, WeeklyPlanDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IDateTimeProvider _clock;

    public ResetWeeklyPlanCommandHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _plans = plans;
        _clock = clock;
    }

    public async Task<WeeklyPlanDto> Handle(ResetWeeklyPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var tracked = await _plans.GetWithSlotsAsync(plan.WeekId, cancellationToken) ?? plan;
        tracked.ResetFuture(tracked.TodayIdx, _clock.UtcNow);
        await _plans.SaveChangesAsync(cancellationToken);
        var refreshed = await _plans.GetWithSlotsAsync(tracked.WeekId, cancellationToken) ?? tracked;
        return PlanMapping.ToDto(refreshed);
    }
}
