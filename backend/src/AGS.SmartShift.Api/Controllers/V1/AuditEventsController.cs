using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Features.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize(Policy = SmartShiftRoles.PolicyHr)]
[Route("api/v1/audit-events")]
public sealed class AuditEventsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditEventsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] TableListRequest table,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ListAuditEventsQuery(table, fromUtc, toUtc),
            cancellationToken);
        return Ok(result);
    }
}
