namespace AGS.SmartShift.Application.Contracts.Staffing;

public sealed class UpsertAircraftManningRuleDto
{
    public string AircraftPattern { get; init; } = string.Empty;
    public int BaseManning { get; init; }
}

public sealed class UpsertAirlineManningRuleDto
{
    public string AirlinePrefix { get; init; } = string.Empty;
    public decimal Multiplier { get; init; } = 1m;
}
