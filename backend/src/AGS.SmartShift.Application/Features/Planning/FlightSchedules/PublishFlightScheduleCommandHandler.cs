using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Notifications;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.FlightSchedules;

public sealed class PublishFlightScheduleCommandHandler
    : IRequestHandler<PublishFlightScheduleCommand, FlightScheduleDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IEmployeeRepository _employees;
    private readonly INotificationRepository _notifications;
    private readonly IDateTimeProvider _clock;

    public PublishFlightScheduleCommandHandler(
        IPlanningWeekService weeks,
        IFlightScheduleRepository schedules,
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _schedules = schedules;
        _employees = employees;
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<FlightScheduleDto> Handle(
        PublishFlightScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var schedule = await _schedules.GetOrCreateForUpdateAsync(
            plan.SiteId,
            plan.WeekId,
            _clock.UtcNow,
            cancellationToken);

        if (schedule.IsLocked)
        {
            throw new DomainException("flight_schedule_already_locked", "Lịch bay tuần này đã được publish và khóa.");
        }

        schedule.PublishAndLock(_clock.UtcNow);
        await _schedules.SaveChangesAsync(cancellationToken);

        await ShiftLeaderNotificationHelper.NotifyShiftLeadersAsync(
            _employees,
            _notifications,
            _clock,
            "flight_schedule_published",
            $"Lịch bay tuần {plan.WeekId} đã publish",
            "TBĐH đã khóa lịch bay. Có thể lập WeeklyPlan trên tab Plan (mobile/web).",
            cancellationToken);

        return new FlightScheduleDto
        {
            WeekId = schedule.WeekId,
            Status = "locked",
            PublishedAtUtc = schedule.PublishedAtUtc,
            LockedAtUtc = schedule.LockedAtUtc,
            IsLocked = true,
        };
    }
}
