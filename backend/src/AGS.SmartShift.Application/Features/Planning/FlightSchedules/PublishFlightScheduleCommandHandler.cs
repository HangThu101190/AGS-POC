using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Notifications;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Planning;
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
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IStaffingSupportQueries _support;
    private readonly IStaffingConfigRepository _staffingConfig;
    private readonly IPlanningDayNotifier _planningNotifier;
    private readonly IDateTimeProvider _clock;

    public PublishFlightScheduleCommandHandler(
        IPlanningWeekService weeks,
        IFlightScheduleRepository schedules,
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IStaffingSupportQueries support,
        IStaffingConfigRepository staffingConfig,
        IPlanningDayNotifier planningNotifier,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _schedules = schedules;
        _employees = employees;
        _notifications = notifications;
        _staffing = staffing;
        _flights = flights;
        _support = support;
        _staffingConfig = staffingConfig;
        _planningNotifier = planningNotifier;
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

        if (schedule.IsLocked && PastDayGuard.IsPastWeek(plan.TodayIdx))
        {
            throw new DomainException(
                "flight_schedule_already_locked",
                "Lịch bay tuần đã qua đã khóa — không thể phát hành lại.");
        }

        if (!schedule.IsLocked)
        {
            schedule.PublishAndLock(_clock.UtcNow);
        }
        await _schedules.SaveChangesAsync(cancellationToken);

        await ShiftLeaderNotificationHelper.NotifyShiftLeadersAsync(
            _employees,
            _notifications,
            _clock,
            "flight_schedule_published",
            $"Lịch bay tuần {plan.WeekId} đã publish",
            "TBĐH đã phát hành lịch bay. Hệ thống đã đề xuất định biên và gán nhân sự — trưởng ca rà soát Bảng phân ca.",
            cancellationToken);

        var staleDays = await StaffingRefreshAfterFlightPublish.RefreshWeekAsync(
            plan.SiteId,
            plan.WeekId,
            _staffing,
            _flights,
            _support,
            _staffingConfig,
            _clock,
            cancellationToken);
        await _planningNotifier.NotifyFlightSchedulePublishedAsync(
            plan.WeekId,
            staleDays,
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
