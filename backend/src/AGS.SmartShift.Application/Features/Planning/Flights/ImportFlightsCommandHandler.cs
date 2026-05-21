using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Notifications;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class ImportFlightsCommandHandler : IRequestHandler<ImportFlightsCommand, FlightImportResultDto>
{
    private readonly IFlightExcelParser _parser;
    private readonly IFlightRepository _flights;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IFlightScheduleDayRepository _scheduleDays;
    private readonly IPlanningWeekService _weeks;
    private readonly IEmployeeRepository _employees;
    private readonly INotificationRepository _notifications;
    private readonly IPlanningDayNotifier _planningNotifier;
    private readonly IDateTimeProvider _clock;

    public ImportFlightsCommandHandler(
        IFlightExcelParser parser,
        IFlightRepository flights,
        IFlightScheduleRepository schedules,
        IFlightScheduleDayRepository scheduleDays,
        IPlanningWeekService weeks,
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IPlanningDayNotifier planningNotifier,
        IDateTimeProvider clock)
    {
        _parser = parser;
        _flights = flights;
        _schedules = schedules;
        _scheduleDays = scheduleDays;
        _weeks = weeks;
        _employees = employees;
        _notifications = notifications;
        _planningNotifier = planningNotifier;
        _clock = clock;
    }

    public async Task<FlightImportResultDto> Handle(
        ImportFlightsCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var parsed = await _parser.ParseAsync(request.FileStream, cancellationToken);
        // Day comes from UI week/day filter (query dayIdx), not from Excel title rows.
        var dayIdx = request.DayIdx ?? plan.TodayIdx;

        PastDayGuard.EnsureMutableDay(dayIdx, plan.TodayIdx, "import lịch bay");
        await FlightScheduleGuard.EnsureMutableForDayAsync(
            _schedules,
            plan.WeekId,
            dayIdx,
            plan.TodayIdx,
            "import lịch bay",
            cancellationToken);
        await _schedules.GetOrCreateForUpdateAsync(plan.SiteId, plan.WeekId, _clock.UtcNow, cancellationToken);

        var entities = parsed.Rows.Select(row =>
            Flight.Create(
                plan.SiteId,
                plan.WeekId,
                dayIdx,
                row.FlightNo,
                row.DepartureFlightNo,
                row.Route,
                row.Sta,
                row.Std,
                row.DepartmentCode,
                row.Manning,
                _clock.UtcNow,
                row.Aircraft,
                row.ManningExplain,
                row.IsVip,
                row.VipNote,
                row.Gate,
                row.Belt,
                row.Parking,
                row.Remark,
                row.Eta,
                row.Etd,
                row.Registration,
                row.Carry,
                row.ExcelRowNo,
                row.SortOrder)).ToList();

        await _flights.ReplaceDayFlightsAsync(plan.WeekId, dayIdx, entities, cancellationToken);

        var sourceLabel = parsed.TargetDayLabel
            ?? plan.GetWeekDates().ElementAtOrDefault(dayIdx);
        var scheduleDay = FlightScheduleDay.Create(
            plan.SiteId,
            plan.WeekId,
            dayIdx,
            _clock.UtcNow,
            sourceLabel,
            parsed.SheetRemark);
        await _scheduleDays.UpsertImportAsync(scheduleDay, cancellationToken);

        var dayLabel = plan.GetWeekDates().ElementAtOrDefault(dayIdx) ?? $"ngày {dayIdx + 1}";
        await ShiftLeaderNotificationHelper.NotifyShiftLeadersAsync(
            _employees,
            _notifications,
            _clock,
            "flight_schedule_ready",
            $"Kế hoạch bay {dayLabel} đã sẵn sàng",
            $"TBĐH đã upload {entities.Count} chuyến. Có thể lập kế hoạch ca tuần (WeeklyPlan).",
            cancellationToken);

        await _planningNotifier.NotifyFlightsDayUpdatedAsync(
            plan.WeekId,
            dayIdx,
            "import",
            entities.Select(f => f.Id).ToList(),
            cancellationToken);

        return new FlightImportResultDto
        {
            ImportedCount = entities.Count,
            DayIdx = dayIdx,
            DayLabel = plan.GetWeekDates().ElementAtOrDefault(dayIdx),
            SheetRemark = parsed.SheetRemark,
            Warnings = parsed.Warnings,
        };
    }
}
