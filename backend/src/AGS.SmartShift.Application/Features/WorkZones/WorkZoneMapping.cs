using AGS.SmartShift.Domain.Entities.Attendance;

namespace AGS.SmartShift.Application.Features.WorkZones;

internal static class WorkZoneMapping
{
    public static WorkZoneDto ToDto(WorkZone zone)
    {
        var ring = zone.GetPolygon();
        return new WorkZoneDto
        {
            Id = zone.Id,
            SiteId = zone.SiteId,
            Name = zone.Name,
            Version = zone.Version,
            Polygon = ring
                .Select(p => new LatLngDto { Lat = p.Lat, Lng = p.Lng })
                .ToList(),
        };
    }
}
