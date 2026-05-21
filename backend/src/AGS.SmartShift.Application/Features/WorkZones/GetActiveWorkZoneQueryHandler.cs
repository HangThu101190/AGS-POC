using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.WorkZones;

public sealed class GetActiveWorkZoneQueryHandler : IRequestHandler<GetActiveWorkZoneQuery, WorkZoneDto?>
{
    private readonly IWorkZoneRepository _workZones;

    public GetActiveWorkZoneQueryHandler(IWorkZoneRepository workZones) => _workZones = workZones;

    public async Task<WorkZoneDto?> Handle(GetActiveWorkZoneQuery request, CancellationToken cancellationToken)
    {
        var siteId = request.SiteId ?? SiteIds.Cxr;
        var zone = await _workZones.GetActiveBySiteIdAsync(siteId, cancellationToken);
        if (zone is not null)
        {
            return WorkZoneMapping.ToDto(zone);
        }

        return new WorkZoneDto
        {
            Id = WorkZoneIds.CxrDefault,
            SiteId = siteId,
            Name = "Khu vực làm việc CXR",
            Version = 0,
            Polygon = WorkZone.CxrDefaultPolygon()
                .Select(p => new LatLngDto { Lat = p.Lat, Lng = p.Lng })
                .ToList(),
        };
    }
}
