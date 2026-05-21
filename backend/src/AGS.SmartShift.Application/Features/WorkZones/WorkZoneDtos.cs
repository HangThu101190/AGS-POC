namespace AGS.SmartShift.Application.Features.WorkZones;

public sealed class WorkZoneDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Version { get; init; }
    public IReadOnlyList<LatLngDto> Polygon { get; init; } = Array.Empty<LatLngDto>();
}

public sealed class LatLngDto
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}
