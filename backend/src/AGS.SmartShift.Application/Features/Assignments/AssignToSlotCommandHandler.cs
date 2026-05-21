using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed class AssignToSlotCommandHandler : IRequestHandler<AssignToSlotCommand, AssignToSlotResult>
{
    private readonly IResourceAccessService _access;
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public AssignToSlotCommandHandler(
        IResourceAccessService access,
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IShiftAssignmentRepository assignments,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _access = access;
        _weeks = weeks;
        _plans = plans;
        _assignments = assignments;
        _employees = employees;
        _clock = clock;
    }

    public async Task<AssignToSlotResult> Handle(AssignToSlotCommand request, CancellationToken cancellationToken)
    {
        if (!_access.CanAccessDepartment(request.DepartmentId))
        {
            throw new ForbiddenAccessException(
                "Supervisor cannot assign employees to slots outside their department.");
        }

        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        if (employee.DepartmentId != request.DepartmentId)
        {
            throw new DomainException("employee_dept_mismatch", "Nhân viên không thuộc phòng ban của slot.");
        }

        var weekPlan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var plan = await _plans.GetWithSlotsForUpdateAsync(weekPlan.WeekId, cancellationToken);

        if (plan is null)
        {
            throw new DomainException("plan_not_found", "Chưa có kế hoạch tuần — publish WeeklyPlan trước.");
        }

        var slot = plan.Slots.FirstOrDefault(s => s.Id == request.SlotId)
            ?? throw new DomainException("slot_not_found", "Không tìm thấy slot ca.");

        PastDayGuard.EnsureMutableDay(slot.DayIdx, plan.TodayIdx, "phân công NV");

        var existingOnSlot = await _assignments.ListBySlotIdsAsync([slot.Id], cancellationToken);
        if (existingOnSlot.Any(a => a.EmployeeId == request.EmployeeId))
        {
            return new AssignToSlotResult { Accepted = true, Message = "Nhân viên đã được gán vào slot này." };
        }

        if (existingOnSlot.Count >= slot.Headcount)
        {
            throw new DomainException("slot_full", "Slot đã đủ định biên.");
        }

        var assignment = ShiftAssignment.Create(
            slot.Id,
            request.EmployeeId,
            slot.GetFlightNos(),
            _clock.UtcNow);
        await _assignments.AddAsync(assignment, cancellationToken);

        plan.MarkInProgress(_clock.UtcNow);
        await _plans.SaveChangesAsync(cancellationToken);

        return new AssignToSlotResult
        {
            Accepted = true,
            Message = "Đã gán nhân viên vào slot.",
            AssignmentId = assignment.Id,
        };
    }
}
