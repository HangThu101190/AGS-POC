using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Leave;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/leave")]
public sealed class LeaveController : ControllerBase
{
    private readonly ISender _sender;

    public LeaveController(ISender sender) => _sender = sender;

    [HttpGet("types")]
    [Authorize]
    public async Task<IActionResult> ListTypes(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListLeaveTypesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("requests")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListLeaveRequestsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("requests")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaff)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveRequestDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateLeaveRequestCommand(body), cancellationToken);
        return Ok(result);
    }

    [HttpPost("requests/{id:guid}/approve")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveLeaveRequestCommand(id), cancellationToken);
        return Ok(result);
    }
}
