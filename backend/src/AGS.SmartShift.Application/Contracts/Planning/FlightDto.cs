namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightDto
{
    public Guid Id { get; init; }
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public int ExcelRowNo { get; init; }
    public int SortOrder { get; init; }
    public string FlightNo { get; init; } = string.Empty;
    public string? DepartureFlightNo { get; init; }
    public string Route { get; init; } = string.Empty;
    public string Sta { get; init; } = string.Empty;
    public string Std { get; init; } = string.Empty;
    public string? Eta { get; init; }
    public string? Etd { get; init; }
    public int EtaDelayMinutes { get; init; }
    public int EtdDelayMinutes { get; init; }
    public int DelayMinutes { get; init; }
    public bool IsDelayed { get; init; }
    public string? Registration { get; init; }
    public string? Aircraft { get; init; }
    public string? Carry { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public int Manning { get; init; }
    public string? ManningExplain { get; init; }
    public bool IsVip { get; init; }
    public string? Gate { get; init; }
    public string? Belt { get; init; }
    public string? Parking { get; init; }
    public string? Remark { get; init; }
}
