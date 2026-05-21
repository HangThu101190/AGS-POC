using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed record RemoveAssignmentCommand(string WeekId, Guid AssignmentId) : IRequest<Unit>;
