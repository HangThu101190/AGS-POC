namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightImportResultDto
{
    public int ImportedCount { get; init; }
    public int? DayIdx { get; init; }
    public string? DayLabel { get; init; }
    public string? SheetRemark { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
