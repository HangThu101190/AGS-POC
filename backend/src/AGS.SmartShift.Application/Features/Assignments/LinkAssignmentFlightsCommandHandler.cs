using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed class LinkAssignmentFlightsCommandHandler
    : IRequestHandler<LinkAssignmentFlightsCommand, LinkAssignmentFlightsResult>
{
    private readonly IResourceAccessService _access;
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IFlightRepository _flights;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public LinkAssignmentFlightsCommandHandler(
        IResourceAccessService access,
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IShiftAssignmentRepository assignments,
        IFlightRepository flights,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _access = access;
        _weeks = weeks;
        _plans = plans;
        _assignments = assignments;
        _flights = flights;
        _employees = employees;
        _clock = clock;
    }

    public async Task<LinkAssignmentFlightsResult> Handle(
        LinkAssignmentFlightsCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignments.GetByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new DomainException("assignment_not_found", "Không tìm thấy phân công.");

        var employee = await _employees.GetByIdAsync(assignment.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        if (!_access.CanAccessDepartment(employee.DepartmentId))
        {
            throw new ForbiddenAccessException(
                "Supervisor cannot link flights for assignments outside their department.");
        }

        var weekPlan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var plan = await _plans.GetWithSlotsAsync(weekPlan.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        var slot = plan.Slots.FirstOrDefault(s => s.Id == assignment.ShiftSlotId)
            ?? throw new DomainException("slot_not_found", "Không tìm thấy slot ca.");

        PastDayGuard.EnsureMutableDay(slot.DayIdx, plan.TodayIdx, "gán chuyến bay");

        var normalized = request.FlightNos
            .Select(f => f.Trim().ToUpperInvariant())
            .Where(f => f.Length > 0)
            .Distinct()
            .ToList();

        var dayFlights = await _flights.ListByWeekAsync(plan.WeekId, cancellationToken);
        var flightsForDay = dayFlights
            .Where(f => f.DayIdx == slot.DayIdx)
            .Where(f => string.IsNullOrWhiteSpace(slot.DepartmentCode)
                || f.DepartmentCode.Equals(slot.DepartmentCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var byNo = flightsForDay.ToDictionary(f => f.FlightNo, StringComparer.OrdinalIgnoreCase);
        var segments = slot.GetSegments();
        var current = assignment.GetFlightNos().ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var flightNo in normalized)
        {
            if (!byNo.TryGetValue(flightNo, out var flight))
            {
                throw new DomainException("flight_not_found", $"Không tìm thấy chuyến {flightNo} trong ngày.");
            }

            if (current.Contains(flightNo))
            {
                continue;
            }

            if (!FlightSlotOverlap.OverlapsSlotSegments(
                    flight.Sta,
                    flight.Std,
                    flight.IsDelayed,
                    flight.Eta,
                    flight.Etd,
                    segments))
            {
                throw new DomainException(
                    "flight_slot_mismatch",
                    $"Chuyến {flightNo} không giao với khung giờ ca.");
            }
        }

        assignment.LinkFlights(normalized, _clock.UtcNow);
        await _assignments.SaveChangesAsync(cancellationToken);

        return new LinkAssignmentFlightsResult
        {
            Accepted = true,
            Message = "Đã cập nhật chuyến bay gán cho nhân viên.",
            FlightNos = assignment.GetFlightNos(),
        };
    }
}
