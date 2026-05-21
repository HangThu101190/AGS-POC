using MediatR;

namespace AGS.SmartShift.Application.Features.WorkZones;

public sealed record UpdateWorkZonePolygonCommand(
    IReadOnlyList<LatLngDto> Polygon) : IRequest<WorkZoneDto>;
