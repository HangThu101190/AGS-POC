namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class SyncProposalDto
{
    public Guid Id { get; init; }
    public Guid FlightId { get; init; }
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public string FlightNo { get; init; } = string.Empty;
    public int DelayMinutes { get; init; }
    public string Status { get; init; } = "pending";
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<SyncProposalAffectedDto> Affected { get; init; } = [];
}

public sealed class SyncProposalAffectedDto
{
    public Guid SlotId { get; init; }
    public Guid? AssignmentId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string[] CurrentSegments { get; init; } = [];
    public string[] ProposedSegments { get; init; } = [];
}
