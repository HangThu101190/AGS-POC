using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Planning.Plans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/plans")]
public sealed class PlansController : ControllerBase
{
    private readonly ISender _sender;

    public PlansController(ISender sender) => _sender = sender;

    [HttpGet("{weekId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWeeklyPlanQuery(weekId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{weekId}/slots/generate")]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateSlots(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GeneratePlanSlotsCommand(weekId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{weekId}/publish")]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishWeeklyPlanCommand(weekId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{weekId}/reset")]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(string weekId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetWeeklyPlanCommand(weekId), cancellationToken);
        return Ok(result);
    }
}
