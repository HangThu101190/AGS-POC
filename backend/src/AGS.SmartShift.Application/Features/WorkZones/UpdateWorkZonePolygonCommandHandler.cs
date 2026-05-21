using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.WorkZones;

public sealed class UpdateWorkZonePolygonCommandHandler
    : IRequestHandler<UpdateWorkZonePolygonCommand, WorkZoneDto>
{
    private readonly IWorkZoneRepository _workZones;
    private readonly IDateTimeProvider _clock;

    public UpdateWorkZonePolygonCommandHandler(IWorkZoneRepository workZones, IDateTimeProvider clock)
    {
        _workZones = workZones;
        _clock = clock;
    }

    public async Task<WorkZoneDto> Handle(UpdateWorkZonePolygonCommand request, CancellationToken cancellationToken)
    {
        if (request.Polygon.Count < 3)
        {
            throw new DomainException("invalid_polygon", "Polygon requires at least 3 vertices.");
        }

        var ring = request.Polygon.Select(p => (p.Lat, p.Lng)).ToList();
        var now = _clock.UtcNow;

        var zone = await _workZones.GetActiveBySiteIdAsync(SiteIds.Cxr, cancellationToken);
        if (zone is null)
        {
            zone = WorkZone.Create(
                WorkZoneIds.CxrDefault,
                SiteIds.Cxr,
                "Khu vực làm việc CXR",
                1,
                ring,
                now);
            await _workZones.AddAsync(zone, cancellationToken);
        }
        else
        {
            var tracked = await _workZones.GetByIdAsync(zone.Id, cancellationToken)
                ?? throw new DomainException("work_zone_not_found", "Active work zone not found.");
            tracked.UpdatePolygon(ring, now);
            zone = tracked;
        }

        await _workZones.SaveChangesAsync(cancellationToken);
        return WorkZoneMapping.ToDto(zone);
    }
}
