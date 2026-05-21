using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed record AssignToSlotCommand(
    string? WeekId,
    Guid DepartmentId,
    Guid SlotId,
    Guid EmployeeId) : IRequest<AssignToSlotResult>;

public sealed class AssignToSlotResult
{
    public bool Accepted { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AssignmentId { get; init; }
}
