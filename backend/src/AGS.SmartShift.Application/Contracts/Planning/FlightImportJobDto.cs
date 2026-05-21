using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightImportJobDto
{
    public Guid Id { get; init; }
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public FlightImportJobStatus Status { get; init; }
    public int ProgressPercent { get; init; }
    public string? ErrorMessage { get; init; }
    public FlightImportResultDto? Result { get; init; }
}
