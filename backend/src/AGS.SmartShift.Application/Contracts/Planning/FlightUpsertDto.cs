namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class FlightUpsertDto
{
    public string? WeekId { get; init; }
    public int DayIdx { get; init; }
    public required string FlightNo { get; init; }
    public string? DepartureFlightNo { get; init; }
    public required string Route { get; init; }
    public required string Sta { get; init; }
    public required string Std { get; init; }
    public required string DepartmentCode { get; init; }
    public int Manning { get; init; } = 2;
    public string? Aircraft { get; init; }
    public string? ManningExplain { get; init; }
    public bool IsVip { get; init; }
    public string? VipNote { get; init; }
    public string? Gate { get; init; }
    public string? Belt { get; init; }
    public string? Parking { get; init; }
    public string? Remark { get; init; }
}
