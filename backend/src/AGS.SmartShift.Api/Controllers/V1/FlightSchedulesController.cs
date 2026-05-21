using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Planning.FlightSchedules;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/flight-schedules")]
public sealed class FlightSchedulesController : ControllerBase
{
    private readonly ISender _sender;

    public FlightSchedulesController(ISender sender) => _sender = sender;

    [HttpGet("{weekId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFlightScheduleQuery(weekId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{weekId}/publish")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishFlightScheduleCommand(weekId), cancellationToken);
        return Ok(result);
    }
}
