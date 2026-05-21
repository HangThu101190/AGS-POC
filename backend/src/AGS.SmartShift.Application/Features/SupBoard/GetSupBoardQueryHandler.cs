using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.SupBoard;

public sealed class GetSupBoardQueryHandler : IRequestHandler<GetSupBoardQuery, SupBoardDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IFlightRepository _flights;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IEmployeeRepository _employees;

    public GetSupBoardQueryHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IFlightRepository flights,
        IShiftAssignmentRepository assignments,
        IEmployeeRepository employees)
    {
        _weeks = weeks;
        _plans = plans;
        _flights = flights;
        _assignments = assignments;
        _employees = employees;
    }

    public async Task<SupBoardDto> Handle(GetSupBoardQuery request, CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var tracked = await _plans.GetWithSlotsAsync(plan.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        var dayIdx = request.DayIdx ?? plan.TodayIdx;
        var slots = tracked.Slots
            .Where(s => s.DayIdx == dayIdx)
            .Where(s => string.IsNullOrWhiteSpace(request.DepartmentCode)
                || s.DepartmentCode.Equals(request.DepartmentCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var slotIds = slots.Select(s => s.Id).ToList();
        var allAssignments = await _assignments.ListBySlotIdsAsync(slotIds, cancellationToken);
        var empCache = new Dictionary<Guid, Domain.Entities.Identity.Employee>();

        var slotDtos = new List<SupBoardSlotDto>();
        foreach (var slot in slots)
        {
            var slotAssignments = allAssignments.Where(a => a.ShiftSlotId == slot.Id).ToList();
            var assignmentDtos = new List<SupBoardAssignmentDto>();
            foreach (var a in slotAssignments)
            {
                if (!empCache.TryGetValue(a.EmployeeId, out var emp))
                {
                    emp = await _employees.GetByIdAsync(a.EmployeeId, cancellationToken);
                    if (emp is not null)
                    {
                        empCache[a.EmployeeId] = emp;
                    }
                }

                assignmentDtos.Add(new SupBoardAssignmentDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = emp?.Name ?? a.EmployeeId.ToString(),
                    EmployeeCode = emp?.Code ?? "",
                    FlightNos = a.GetFlightNos().ToArray(),
                });
            }

            slotDtos.Add(new SupBoardSlotDto
            {
                Id = slot.Id,
                DepartmentCode = slot.DepartmentCode,
                DayIdx = slot.DayIdx,
                Segments = slot.GetSegments().ToArray(),
                Headcount = slot.Headcount,
                FlightNos = slot.GetFlightNos().ToArray(),
                Assignments = assignmentDtos,
            });
        }

        var flightPage = await _flights.ListAsync(
            plan.WeekId,
            dayIdx,
            request.DepartmentCode,
            new TableListCriteria { Page = 0, PageSize = 500 },
            cancellationToken);

        return new SupBoardDto
        {
            WeekId = plan.WeekId,
            DayIdx = dayIdx,
            PlanStatus = tracked.Status.ToString().ToLowerInvariant(),
            Slots = slotDtos,
            Flights = flightPage.Items
                .Select(f => new SupBoardFlightDto
                {
                    Id = f.Id,
                    FlightNo = f.FlightNo,
                    Route = f.Route,
                    Sta = f.Sta,
                    Std = f.Std,
                    IsDelayed = f.IsDelayed,
                    DelayMinutes = f.DelayMinutes,
                })
                .ToList(),
        };
    }
}
