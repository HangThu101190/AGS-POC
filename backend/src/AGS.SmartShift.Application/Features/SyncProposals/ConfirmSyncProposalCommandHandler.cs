using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.SyncProposals;

public sealed class ConfirmSyncProposalCommandHandler : IRequestHandler<ConfirmSyncProposalCommand, SyncProposalDto>
{
    private readonly ISyncProposalRepository _proposals;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IEmployeeRepository _employees;
    private readonly IWeeklyPlanRepository _plans;
    private readonly INotificationRepository _notifications;
    private readonly IStaffShiftNotifier _staffNotifier;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public ConfirmSyncProposalCommandHandler(
        ISyncProposalRepository proposals,
        IShiftAssignmentRepository assignments,
        IEmployeeRepository employees,
        IWeeklyPlanRepository plans,
        INotificationRepository notifications,
        IStaffShiftNotifier staffNotifier,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _proposals = proposals;
        _assignments = assignments;
        _employees = employees;
        _plans = plans;
        _notifications = notifications;
        _staffNotifier = staffNotifier;
        _user = user;
        _clock = clock;
    }

    public async Task<SyncProposalDto> Handle(
        ConfirmSyncProposalCommand request,
        CancellationToken cancellationToken)
    {
        var proposal = await _proposals.GetByIdAsync(request.ProposalId, cancellationToken)
            ?? throw new DomainException("sync_proposal_not_found", "Không tìm thấy đề xuất sync ca.");

        var byEmpId = _user.EmployeeId
            ?? throw new DomainException("employee_required", "Thiếu thông tin nhân viên đăng nhập.");

        var byEmp = await _employees.GetByIdAsync(byEmpId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        proposal.Confirm(byEmpId, _clock.UtcNow);

        var plan = await _plans.GetWithSlotsForUpdateAsync(proposal.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        var slotUpdates = proposal.GetAffected()
            .GroupBy(l => l.SlotId)
            .ToDictionary(g => g.Key, g => g.First().ProposedSegments);

        foreach (var slot in plan.Slots.Where(s => slotUpdates.ContainsKey(s.Id)))
        {
            slot.ApplySegments(slotUpdates[slot.Id]);
        }

        var staffNotes = new List<NotificationMessage>();
        var notifyEmployeeIds = new List<Guid>();
        foreach (var line in proposal.GetAffected())
        {
            if (line.AssignmentId is not Guid assignmentId)
            {
                continue;
            }

            var revision = ShiftRevision.Create(
                assignmentId,
                proposal.Summary,
                proposal.FlightNo,
                byEmp.Code,
                line.ProposedSegments,
                _clock.UtcNow);
            await _proposals.AddRevisionAsync(revision, cancellationToken);

            var assignment = await _assignments.GetByIdAsync(assignmentId, cancellationToken);
            if (assignment is not null)
            {
                notifyEmployeeIds.Add(assignment.EmployeeId);
                staffNotes.Add(
                    NotificationMessage.Create(
                        assignment.EmployeeId,
                        "shift_changed",
                        "Ca đã thay đổi",
                        $"Chuyến {proposal.FlightNo} delay {proposal.DelayMinutes}' — vui lòng xác nhận trước khi check-in.",
                        _clock.UtcNow));
            }
        }

        if (staffNotes.Count > 0)
        {
            await _notifications.AddRangeAsync(staffNotes, cancellationToken);
        }

        await _plans.SaveChangesAsync(cancellationToken);
        await _proposals.SaveChangesAsync(cancellationToken);

        if (notifyEmployeeIds.Count > 0)
        {
            await _staffNotifier.NotifyShiftChangedAsync(
                notifyEmployeeIds,
                proposal.FlightNo,
                proposal.DelayMinutes,
                cancellationToken);
        }
        return SyncProposalMapping.ToDto(proposal);
    }
}
