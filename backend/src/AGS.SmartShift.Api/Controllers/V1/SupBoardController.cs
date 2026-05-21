using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.SupBoard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize(Policy = SmartShiftRoles.PolicySup)]
[Route("api/v1/supboard")]
public sealed class SupBoardController : ControllerBase
{
    private readonly ISender _sender;

    public SupBoardController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? weekId,
        [FromQuery] int? dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetSupBoardQuery(weekId, dayIdx, departmentCode),
            cancellationToken);
        return Ok(result);
    }
}
