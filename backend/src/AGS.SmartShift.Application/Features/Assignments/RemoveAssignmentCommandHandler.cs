using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed class RemoveAssignmentCommandHandler : IRequestHandler<RemoveAssignmentCommand, Unit>
{
    private readonly IResourceAccessService _access;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IEmployeeRepository _employees;

    public RemoveAssignmentCommandHandler(
        IResourceAccessService access,
        IShiftAssignmentRepository assignments,
        IWeeklyPlanRepository plans,
        IEmployeeRepository employees)
    {
        _access = access;
        _assignments = assignments;
        _plans = plans;
        _employees = employees;
    }

    public async Task<Unit> Handle(RemoveAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignments.GetByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new DomainException("assignment_not_found", "Không tìm thấy phân công.");

        var emp = await _employees.GetByIdAsync(assignment.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        if (!_access.CanAccessDepartment(emp.DepartmentId))
        {
            throw new ForbiddenAccessException("Không có quyền xóa phân công ngoài phòng ban.");
        }

        var plan = await _plans.GetWithSlotsAsync(request.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        var slot = plan.Slots.FirstOrDefault(s => s.Id == assignment.ShiftSlotId)
            ?? throw new DomainException("slot_not_found", "Không tìm thấy slot.");

        PastDayGuard.EnsureMutableDay(slot.DayIdx, plan.TodayIdx, "gỡ phân công NV");
        await _assignments.RemoveAsync(assignment, cancellationToken);
        return Unit.Value;
    }
}
