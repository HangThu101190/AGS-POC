using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.WorkZones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/work-zones")]
public sealed class WorkZonesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkZonesController(ISender sender) => _sender = sender;

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var zone = await _sender.Send(new GetActiveWorkZoneQuery(siteId), cancellationToken);
        return Ok(zone);
    }

    [HttpPut("active/polygon")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePolygon(
        [FromBody] UpdatePolygonRequest body,
        CancellationToken cancellationToken)
    {
        var polygon = body.Polygon
            .Select(p => new LatLngDto { Lat = p.Lat, Lng = p.Lng })
            .ToList();
        var zone = await _sender.Send(new UpdateWorkZonePolygonCommand(polygon), cancellationToken);
        return Ok(zone);
    }

    public sealed class UpdatePolygonRequest
    {
        public List<LatLngBody> Polygon { get; init; } = [];
    }

    public sealed class LatLngBody
    {
        public double Lat { get; init; }
        public double Lng { get; init; }
    }
}
