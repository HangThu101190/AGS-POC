using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

public sealed record GetStaffingDayQuery(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<StaffingDayDto>;

public sealed class GetStaffingDayQueryHandler : IRequestHandler<GetStaffingDayQuery, StaffingDayDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IFlightRepository _flights;
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IAttendanceRepository _attendance;
    private readonly IStaffingSupportQueries _support;
    private readonly AttendanceComplianceEvaluator _compliance = new();

    public GetStaffingDayQueryHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IAttendanceRepository attendance,
        IStaffingSupportQueries support)
    {
        _weeks = weeks;
        _staffing = staffing;
        _flights = flights;
        _employees = employees;
        _departments = departments;
        _attendance = attendance;
        _support = support;
    }

    public async Task<StaffingDayDto> Handle(GetStaffingDayQuery request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetOrCreatePlanAsync(
            week.SiteId,
            week.WeekId,
            request.DayIdx,
            dept,
            DateTime.UtcNow,
            cancellationToken);

        var lines = await _staffing.ListLinesAsync(plan.Id, cancellationToken);
        var dayFlights = (await _flights.ListByWeekAsync(week.WeekId, cancellationToken))
            .Where(f => f.DayIdx == request.DayIdx && f.DepartmentCode == dept)
            .ToList();
        var flightById = dayFlights.ToDictionary(f => f.Id);
        var assignments = await _staffing.ListAssignmentsForPlanAsync(plan.Id, cancellationToken);
        var employeePage = await _employees.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 500 },
            cancellationToken);
        var employees = employeePage.Items;
        var empById = employees.ToDictionary(e => e.Id);
        var deptPage = await _departments.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 200 },
            cancellationToken);
        var deptCodeById = StaffingMapping.DepartmentCodesById(deptPage.Items);

        var policies = await _support.ListShiftCheckInPoliciesAsync(week.SiteId, dept, cancellationToken);
        var activeAttendance = await _attendance.ListActiveWithCoordinatesAsync(cancellationToken);

        var assignmentDtos = assignments.Select(a =>
        {
            AttendanceComplianceDto? compliance = null;
            var record = activeAttendance.FirstOrDefault(r =>
                r.EmployeeId == a.EmployeeId
                && (r.FlightCrewAssignmentId == a.Id || r.FlightCrewAssignmentId == null));
            var line = lines.FirstOrDefault(l => l.Id == a.StaffingLineId);
            Flight? flight = line != null && flightById.TryGetValue(line.FlightId, out var f) ? f : null;
            var policy = policies.FirstOrDefault();
            if (flight != null)
            {
                compliance = _compliance.Evaluate(a, record, policy, flight);
            }

            empById.TryGetValue(a.EmployeeId, out var emp);
            return new StaffingAssignmentDto
            {
                Id = a.Id,
                StaffingLineId = a.StaffingLineId,
                EmployeeId = a.EmployeeId,
                EmployeeCode = emp?.Code ?? string.Empty,
                EmployeeName = emp?.Name ?? string.Empty,
                EmployeeDepartmentCode = StaffingMapping.DepartmentCodeForEmployee(emp, deptCodeById),
                Role = a.Role,
                WorkStart = a.WorkStart.ToString("HH:mm"),
                WorkEnd = a.WorkEnd.ToString("HH:mm"),
                IsOvertime = a.IsOvertime,
                Attendance = compliance,
            };
        }).ToList();

        return new StaffingDayDto
        {
            Plan = StaffingMapping.ToPlanDto(plan),
            Lines = lines.Select(StaffingMapping.ToLineDto).ToList(),
            Flights = dayFlights.Select(StaffingMapping.ToFlightDto).ToList(),
            Assignments = assignmentDtos,
        };
    }
}
