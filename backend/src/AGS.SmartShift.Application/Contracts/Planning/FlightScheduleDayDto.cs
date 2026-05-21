namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightScheduleDayDto
{
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public string? SourceDayLabel { get; init; }
    public string? SheetRemark { get; init; }
}
