using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Notifications;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed class PublishWeeklyPlanCommandHandler : IRequestHandler<PublishWeeklyPlanCommand, WeeklyPlanDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IEmployeeRepository _employees;
    private readonly INotificationRepository _notifications;
    private readonly IDateTimeProvider _clock;

    public PublishWeeklyPlanCommandHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _plans = plans;
        _employees = employees;
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<WeeklyPlanDto> Handle(
        PublishWeeklyPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var tracked = await _plans.GetWithSlotsForUpdateAsync(plan.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        if (tracked.Slots.Count == 0)
        {
            throw new DomainException("plan_no_slots", "Chưa có slot ca — hãy sinh slot trước khi phát hành.");
        }

        tracked.Publish(_clock.UtcNow);
        await _plans.SaveChangesAsync(cancellationToken);

        await CreatePublishNotificationsAsync(tracked, cancellationToken);
        await ShiftLeaderNotificationHelper.NotifyShiftLeadersAsync(
            _employees,
            _notifications,
            _clock,
            "plan_published_sl",
            "Kế hoạch ca tuần đã phát hành",
            $"Tuần {tracked.WeekId}: slot ca đã publish — tiếp tục phân công NV (Assign).",
            cancellationToken);

        var refreshed = await _plans.GetWithSlotsAsync(tracked.WeekId, cancellationToken) ?? tracked;
        return PlanMapping.ToDto(refreshed);
    }

    private async Task CreatePublishNotificationsAsync(WeeklyPlan plan, CancellationToken cancellationToken)
    {
        var criteria = new Domain.Common.TableListCriteria { Page = 0, PageSize = 500 };
        var staffPage = await _employees.ListAsync(criteria, cancellationToken);
        var messages = staffPage.Items
            .Where(e => e.IsActive)
            .Select(e => NotificationMessage.Create(
                e.Id,
                "plan_published",
                "Kế hoạch tuần đã phát hành",
                $"Tuần {plan.WeekId} đã phát hành. Vui lòng xem lịch ca trên mobile.",
                _clock.UtcNow))
            .ToList();

        if (messages.Count > 0)
        {
            await _notifications.AddRangeAsync(messages, cancellationToken);
        }
    }
}
