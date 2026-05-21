using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.SyncProposals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/sync-proposals")]
public sealed class SyncProposalsController : ControllerBase
{
    private readonly ISender _sender;

    public SyncProposalsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? weekId,
        [FromQuery] bool pendingOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSyncProposalsQuery(weekId, pendingOnly), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConfirmSyncProposalCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Policy = SmartShiftRoles.PolicySup)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DismissSyncProposalCommand(id), cancellationToken);
        return Ok(result);
    }
}
