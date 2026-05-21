using MediatR;

namespace AGS.SmartShift.Application.Features.Assignments;

public sealed record LinkAssignmentFlightsCommand(
    string? WeekId,
    Guid AssignmentId,
    IReadOnlyList<string> FlightNos) : IRequest<LinkAssignmentFlightsResult>;

public sealed class LinkAssignmentFlightsResult
{
    public bool Accepted { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> FlightNos { get; init; } = [];
}
