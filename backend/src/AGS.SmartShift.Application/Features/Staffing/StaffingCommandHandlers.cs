using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

public sealed record ProposeStaffingDayCommand(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<StaffingDayDto>;

public sealed class ProposeStaffingDayCommandHandler : IRequestHandler<ProposeStaffingDayCommand, StaffingDayDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IStaffingSupportQueries _support;
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;

    public ProposeStaffingDayCommandHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IStaffingSupportQueries support,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock,
        ISender sender)
    {
        _weeks = weeks;
        _staffing = staffing;
        _flights = flights;
        _support = support;
        _notifier = notifier;
        _clock = clock;
        _sender = sender;
    }

    public async Task<StaffingDayDto> Handle(ProposeStaffingDayCommand request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetOrCreatePlanAsync(
            week.SiteId, week.WeekId, request.DayIdx, dept, _clock.UtcNow, cancellationToken);
        var flights = (await _flights.ListByWeekAsync(week.WeekId, cancellationToken))
            .Where(f => f.DayIdx == request.DayIdx && f.DepartmentCode == dept)
            .ToList();
        var aircraftRules = await _support.ListAircraftManningRulesAsync(week.SiteId, cancellationToken);
        var airlineRules = await _support.ListAirlineManningRulesAsync(week.SiteId, cancellationToken);
        var lines = ManningProposeService.BuildLines(plan, flights, aircraftRules, airlineRules, _clock.UtcNow);
        await _staffing.ReplaceLinesAsync(plan.Id, lines, removeOrphanAssignments: true, cancellationToken);
        plan.SetStatus(DailyStaffingPlanStatus.Proposed, _clock.UtcNow);
        await _staffing.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            week.WeekId, request.DayIdx, "propose", plan.Status.ToString(), cancellationToken);
        return await _sender.Send(
            new GetStaffingDayQuery(week.WeekId, request.DayIdx, dept),
            cancellationToken);
    }
}

public sealed record PatchStaffingLineCommand(Guid LineId, PatchStaffingLineDto Body) : IRequest<StaffingLineDto>;

public sealed class PatchStaffingLineCommandHandler : IRequestHandler<PatchStaffingLineCommand, StaffingLineDto>
{
    private readonly IDailyStaffingRepository _staffing;
    private readonly IDateTimeProvider _clock;

    public PatchStaffingLineCommandHandler(IDailyStaffingRepository staffing, IDateTimeProvider clock)
    {
        _staffing = staffing;
        _clock = clock;
    }

    public async Task<StaffingLineDto> Handle(PatchStaffingLineCommand request, CancellationToken cancellationToken)
    {
        var line = await _staffing.GetLineByIdAsync(request.LineId, cancellationToken)
            ?? throw new DomainException("line_not_found", "Không tìm thấy dòng phân công.");
        line.SetTargetManning(request.Body.TargetManning, _clock.UtcNow);
        await _staffing.SaveChangesAsync(cancellationToken);
        return StaffingMapping.ToLineDto(line);
    }
}

public sealed record CreateStaffingAssignmentCommand(CreateStaffingAssignmentDto Body)
    : IRequest<StaffingAssignmentDto>;

public sealed class CreateStaffingAssignmentCommandHandler
    : IRequestHandler<CreateStaffingAssignmentCommand, StaffingAssignmentDto>
{
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IEmployeeRepository _employees;
    private readonly IStaffingSupportQueries _support;
    private readonly AssignmentConflictChecker _conflicts = new();
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;

    public CreateStaffingAssignmentCommandHandler(
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IEmployeeRepository employees,
        IStaffingSupportQueries support,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock,
        ISender sender)
    {
        _staffing = staffing;
        _flights = flights;
        _employees = employees;
        _support = support;
        _notifier = notifier;
        _clock = clock;
        _sender = sender;
    }

    public async Task<StaffingAssignmentDto> Handle(
        CreateStaffingAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var body = request.Body;
        var line = await _staffing.GetLineByIdAsync(body.StaffingLineId, cancellationToken)
            ?? throw new DomainException("line_not_found", "Không tìm thấy dòng phân công.");
        var flight = await _flights.GetByIdAsync(line.FlightId, cancellationToken)
            ?? throw new DomainException("flight_not_found", "Không tìm thấy chuyến bay.");
        var employee = await _employees.GetByIdAsync(body.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");
        var planAssignments = await _staffing.ListAssignmentsForPlanAsync(line.DailyStaffingPlanId, cancellationToken);
        var forEmployee = planAssignments.Where(a => a.EmployeeId == body.EmployeeId).ToList();
        var forLine = planAssignments.Where(a => a.StaffingLineId == line.Id).ToList();
        var avail = await _support.ListDayAvailabilitiesAsync(flight.WeekId, flight.DayIdx, cancellationToken);
        var eligible = !avail.Any(a => a.EmployeeId == body.EmployeeId && a.Status != DayAvailabilityStatus.Available);
        var workStart = TimeOnly.Parse(body.WorkStart);
        var workEnd = TimeOnly.Parse(body.WorkEnd);
        _conflicts.EnsureCanAssign(line, flight, forEmployee, forLine, workStart, workEnd, body.IsOvertime, eligible);
        var assignment = FlightCrewAssignment.Create(
            line.Id,
            body.EmployeeId,
            body.Role,
            workStart,
            workEnd,
            body.IsOvertime,
            forLine.Count + 1,
            _clock.UtcNow);
        await _staffing.AddAssignmentAsync(assignment, cancellationToken);
        await _staffing.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            flight.WeekId, flight.DayIdx, "assign", DailyStaffingPlanStatus.CrewDraft.ToString(), cancellationToken);
        var day = await _sender.Send(new GetStaffingDayQuery(flight.WeekId, flight.DayIdx, flight.DepartmentCode), cancellationToken);
        return day.Assignments.First(a => a.Id == assignment.Id);
    }
}

public sealed record UpdateStaffingAssignmentCommand(Guid AssignmentId, UpdateStaffingAssignmentDto Body)
    : IRequest<StaffingAssignmentDto>;

public sealed class UpdateStaffingAssignmentCommandHandler
    : IRequestHandler<UpdateStaffingAssignmentCommand, StaffingAssignmentDto>
{
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly AssignmentConflictChecker _conflicts = new();
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;

    public UpdateStaffingAssignmentCommandHandler(
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock,
        ISender sender)
    {
        _staffing = staffing;
        _flights = flights;
        _notifier = notifier;
        _clock = clock;
        _sender = sender;
    }

    public async Task<StaffingAssignmentDto> Handle(
        UpdateStaffingAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await _staffing.GetAssignmentByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new DomainException("assignment_not_found", "Không tìm thấy phân công.");
        var line = await _staffing.GetLineByIdAsync(assignment.StaffingLineId, cancellationToken)
            ?? throw new DomainException("line_not_found", "Không tìm thấy dòng phân công.");
        var flight = await _flights.GetByIdAsync(line.FlightId, cancellationToken)
            ?? throw new DomainException("flight_not_found", "Không tìm thấy chuyến bay.");
        var planAssignments = await _staffing.ListAssignmentsForPlanAsync(line.DailyStaffingPlanId, cancellationToken);
        var forEmployee = planAssignments.Where(a => a.EmployeeId == assignment.EmployeeId).ToList();
        var workStart = TimeOnly.Parse(request.Body.WorkStart);
        var workEnd = TimeOnly.Parse(request.Body.WorkEnd);
        _conflicts.EnsureCanUpdateWorkWindow(assignment, forEmployee, workStart, workEnd);
        assignment.Update(request.Body.Role, workStart, workEnd, request.Body.IsOvertime, _clock.UtcNow);
        await _staffing.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            flight.WeekId, flight.DayIdx, "assign", "crew_draft", cancellationToken);
        var day = await _sender.Send(
            new GetStaffingDayQuery(flight.WeekId, flight.DayIdx, flight.DepartmentCode),
            cancellationToken);
        return day.Assignments.First(a => a.Id == assignment.Id);
    }
}

public sealed record DeleteStaffingAssignmentCommand(Guid AssignmentId) : IRequest;

public sealed class DeleteStaffingAssignmentCommandHandler : IRequestHandler<DeleteStaffingAssignmentCommand>
{
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IPlanningDayNotifier _notifier;

    public DeleteStaffingAssignmentCommandHandler(
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IPlanningDayNotifier notifier)
    {
        _staffing = staffing;
        _flights = flights;
        _notifier = notifier;
    }

    public async Task Handle(DeleteStaffingAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _staffing.GetAssignmentByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new DomainException("assignment_not_found", "Không tìm thấy phân công.");
        var line = await _staffing.GetLineByIdAsync(assignment.StaffingLineId, cancellationToken);
        await _staffing.RemoveAssignmentAsync(assignment, cancellationToken);
        await _staffing.SaveChangesAsync(cancellationToken);
        if (line != null)
        {
            var flight = await _flights.GetByIdAsync(line.FlightId, cancellationToken);
            if (flight != null)
            {
                await _notifier.NotifyStaffingDayUpdatedAsync(
                    flight.WeekId, flight.DayIdx, "assign", "crew_draft", cancellationToken);
            }
        }
    }
}

public sealed record ConfirmStaffingDayCommand(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<ConfirmStaffingResultDto>;

public sealed class ConfirmStaffingDayCommandHandler : IRequestHandler<ConfirmStaffingDayCommand, ConfirmStaffingResultDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IDailyStaffingSlotSyncService _slotSync;
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;

    public ConfirmStaffingDayCommandHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IDailyStaffingSlotSyncService slotSync,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock,
        ISender sender)
    {
        _weeks = weeks;
        _staffing = staffing;
        _flights = flights;
        _slotSync = slotSync;
        _notifier = notifier;
        _clock = clock;
        _sender = sender;
    }

    public async Task<ConfirmStaffingResultDto> Handle(
        ConfirmStaffingDayCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetPlanAsync(week.SiteId, week.WeekId, request.DayIdx, dept, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Chưa có kế hoạch phân công ngày.");
        var lines = await _staffing.ListLinesAsync(plan.Id, cancellationToken);
        var assignments = await _staffing.ListAssignmentsForPlanAsync(plan.Id, cancellationToken);
        var flightsById = new Dictionary<Guid, Flight>();
        foreach (var line in lines)
        {
            var flight = await _flights.GetByIdAsync(line.FlightId, cancellationToken);
            if (flight is null)
            {
                continue;
            }

            var manning = assignments.Count(a => a.StaffingLineId == line.Id);
            if (manning < 1)
            {
                manning = line.TargetManning;
            }

            flight.UpdateDetails(
                flight.Route,
                flight.Sta,
                flight.Std,
                flight.DepartmentCode,
                manning,
                _clock.UtcNow,
                flight.DepartureFlightNo,
                flight.Aircraft,
                "daily_staffing_confirm");
            flightsById[flight.Id] = flight;
        }

        var lineToFlight = lines.ToDictionary(l => l.Id, l => l.FlightId);
        var sync = await _slotSync.SyncAsync(
            week.WeekId,
            request.DayIdx,
            dept,
            assignments,
            lineToFlight,
            flightsById,
            _clock.UtcNow,
            cancellationToken);

        plan.SetStatus(DailyStaffingPlanStatus.Confirmed, _clock.UtcNow);
        await _staffing.SaveChangesAsync(cancellationToken);
        await _flights.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            week.WeekId, request.DayIdx, "confirm", plan.Status.ToString(), cancellationToken);
        return new ConfirmStaffingResultDto
        {
            SlotsUpdated = sync.SlotsUpdated,
            AssignmentsCreated = sync.AssignmentsCreated,
            Plan = StaffingMapping.ToPlanDto(plan),
        };
    }
}

public sealed record GetStaffingRosterQuery(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<IReadOnlyList<StaffingRosterEntryDto>>;

public sealed class GetStaffingRosterQueryHandler : IRequestHandler<GetStaffingRosterQuery, IReadOnlyList<StaffingRosterEntryDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IStaffingSupportQueries _support;

    public GetStaffingRosterQueryHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IStaffingSupportQueries support)
    {
        _weeks = weeks;
        _staffing = staffing;
        _employees = employees;
        _departments = departments;
        _support = support;
    }

    public async Task<IReadOnlyList<StaffingRosterEntryDto>> Handle(
        GetStaffingRosterQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetOrCreatePlanAsync(
            week.SiteId, week.WeekId, request.DayIdx, dept, DateTime.UtcNow, cancellationToken);
        var employees = (await _employees.ListAsync(
            new Domain.Common.TableListCriteria { Page = 0, PageSize = 500 },
            cancellationToken)).Items.Where(e => e.DepartmentId != Guid.Empty).ToList();
        var deptPage = await _departments.ListAsync(
            new Domain.Common.TableListCriteria { Page = 0, PageSize = 200 },
            cancellationToken);
        var deptCodeById = StaffingMapping.DepartmentCodesById(deptPage.Items);
        var avail = await _support.ListDayAvailabilitiesAsync(week.WeekId, request.DayIdx, cancellationToken);
        var assignments = await _staffing.ListAssignmentsForPlanAsync(plan.Id, cancellationToken);
        return employees.Select(e =>
        {
            var dayAvail = avail.FirstOrDefault(a => a.EmployeeId == e.Id);
            var eligible = dayAvail is null || dayAvail.Status == DayAvailabilityStatus.Available;
            return new StaffingRosterEntryDto
            {
                EmployeeId = e.Id,
                Code = e.Code,
                Name = e.Name,
                DepartmentCode = StaffingMapping.DepartmentCodeForEmployee(e, deptCodeById),
                Eligible = eligible,
                EligibleAsOvertime = !eligible,
                AvailabilityStatus = dayAvail?.Status,
                AssignedLineIds = assignments.Where(a => a.EmployeeId == e.Id).Select(a => a.StaffingLineId).ToList(),
            };
        }).ToList();
    }
}

public sealed record ExportStaffingDayQuery(string WeekId, int DayIdx, string? DepartmentCode) : IRequest<byte[]>;

public sealed class ExportStaffingDayQueryHandler : IRequestHandler<ExportStaffingDayQuery, byte[]>
{
    private readonly ISender _sender;
    private readonly IPvhkDailyAssignmentExporter _exporter;
    private readonly IPlanningWeekService _weeks;

    public ExportStaffingDayQueryHandler(ISender sender, IPvhkDailyAssignmentExporter exporter, IPlanningWeekService weeks)
    {
        _sender = sender;
        _exporter = exporter;
        _weeks = weeks;
    }

    public async Task<byte[]> Handle(ExportStaffingDayQuery request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var day = await _sender.Send(
            new GetStaffingDayQuery(request.WeekId, request.DayIdx, request.DepartmentCode),
            cancellationToken);
        var label = week.GetWeekDates().ElementAtOrDefault(request.DayIdx) ?? $"Day {request.DayIdx + 1}";
        var year = int.TryParse(request.WeekId.Split('-')[0], out var y) ? y : DateTime.UtcNow.Year;
        return _exporter.Export(new PvhkExportRequest { Day = day, DayLabel = label, CalendarYear = year });
    }
}

public sealed record ExportStaffingWeekQuery(string WeekId, string? DepartmentCode) : IRequest<byte[]>;

public sealed class ExportStaffingWeekQueryHandler : IRequestHandler<ExportStaffingWeekQuery, byte[]>
{
    private readonly ISender _sender;
    private readonly IPvhkDailyAssignmentExporter _exporter;
    private readonly IPlanningWeekService _weeks;

    public ExportStaffingWeekQueryHandler(ISender sender, IPvhkDailyAssignmentExporter exporter, IPlanningWeekService weeks)
    {
        _sender = sender;
        _exporter = exporter;
        _weeks = weeks;
    }

    public async Task<byte[]> Handle(ExportStaffingWeekQuery request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dates = week.GetWeekDates();
        var year = int.TryParse(request.WeekId.Split('-')[0], out var y) ? y : DateTime.UtcNow.Year;
        var dayRequests = new List<PvhkExportRequest>();

        for (var dayIdx = 0; dayIdx < 7; dayIdx++)
        {
            var day = await _sender.Send(
                new GetStaffingDayQuery(request.WeekId, dayIdx, request.DepartmentCode),
                cancellationToken);
            var label = dates.ElementAtOrDefault(dayIdx) ?? $"Day {dayIdx + 1}";
            dayRequests.Add(new PvhkExportRequest { Day = day, DayLabel = label, CalendarYear = year });
        }

        return _exporter.ExportWeek(new PvhkWeekExportRequest { Days = dayRequests });
    }
}
