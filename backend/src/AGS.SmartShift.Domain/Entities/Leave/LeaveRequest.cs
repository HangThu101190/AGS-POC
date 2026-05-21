using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Leave;

public sealed class LeaveRequest : AuditableEntity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public string? Note { get; private set; }

    private LeaveRequest()
    {
    }

    public static LeaveRequest Create(
        Guid employeeId,
        Guid leaveTypeId,
        DateOnly fromDate,
        DateOnly toDate,
        string? note,
        DateTime utcNow)
    {
        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = LeaveRequestStatus.Pending,
            Note = note?.Trim(),
        };
        request.MarkCreated(utcNow);
        return request;
    }

    public void Approve(DateTime utcNow) => SetStatus(LeaveRequestStatus.Approved, utcNow);

    public void Reject(DateTime utcNow) => SetStatus(LeaveRequestStatus.Rejected, utcNow);

    private void SetStatus(LeaveRequestStatus status, DateTime utcNow)
    {
        Status = status;
        MarkUpdated(utcNow);
    }
}
