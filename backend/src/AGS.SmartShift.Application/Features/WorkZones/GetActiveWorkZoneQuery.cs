using MediatR;

namespace AGS.SmartShift.Application.Features.WorkZones;

public sealed record GetActiveWorkZoneQuery(Guid? SiteId = null) : IRequest<WorkZoneDto?>;
