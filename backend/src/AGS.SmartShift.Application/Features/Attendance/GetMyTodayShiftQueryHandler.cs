using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Attendance;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed class GetMyTodayShiftQueryHandler : IRequestHandler<GetMyTodayShiftQuery, StaffTodayShiftDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _user;

    public GetMyTodayShiftQueryHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IShiftAssignmentRepository assignments,
        INotificationRepository notifications,
        ICurrentUserService user)
    {
        _weeks = weeks;
        _plans = plans;
        _assignments = assignments;
        _notifications = notifications;
        _user = user;
    }

    public async Task<StaffTodayShiftDto> Handle(GetMyTodayShiftQuery request, CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new ForbiddenAccessException("Staff account required.");
        }

        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var tracked = await _plans.GetWithSlotsAsync(plan.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        var todaySlots = tracked.Slots.Where(s => s.DayIdx == plan.TodayIdx).ToList();
        var shiftChanged = await _notifications.HasUnreadOfTypeAsync(
            employeeId,
            "shift_changed",
            cancellationToken);

        if (todaySlots.Count == 0)
        {
            return new StaffTodayShiftDto
            {
                PlanStatus = tracked.Status.ToString(),
                ShiftChanged = shiftChanged,
            };
        }

        var slotIds = todaySlots.Select(s => s.Id).ToList();
        var assignments = await _assignments.ListBySlotIdsAsync(slotIds, cancellationToken);
        var mine = assignments.FirstOrDefault(a => a.EmployeeId == employeeId);
        if (mine is null)
        {
            return new StaffTodayShiftDto
            {
                PlanStatus = tracked.Status.ToString(),
                ShiftChanged = shiftChanged,
            };
        }

        var slot = todaySlots.First(s => s.Id == mine.ShiftSlotId);
        var flightNos = mine.GetFlightNos();
        if (flightNos.Count == 0)
        {
            flightNos = slot.GetFlightNos();
        }

        return new StaffTodayShiftDto
        {
            PlanStatus = tracked.Status.ToString(),
            Segments = slot.GetSegments().ToList(),
            FlightNos = flightNos.ToList(),
            ShiftChanged = shiftChanged,
        };
    }
}
