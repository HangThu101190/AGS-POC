using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Monitoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize(Policy = SmartShiftRoles.PolicyHr)]
[Route("api/v1/monitoring")]
public sealed class MonitoringController : ControllerBase
{
    private readonly ISender _sender;

    public MonitoringController(ISender sender) => _sender = sender;

    [HttpGet("snapshot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSnapshot(
        [FromQuery] DateTime? since,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMonitoringSnapshotQuery(since), cancellationToken);
        if (result.Unchanged)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return Ok(result.Snapshot);
    }
}
