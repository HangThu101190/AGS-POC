namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightScheduleDto
{
    public string WeekId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? LockedAtUtc { get; init; }
    public bool IsLocked { get; init; }
}
