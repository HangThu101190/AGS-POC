using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender) => _sender = sender;

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMine(
        [FromQuery] TableListRequest table,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListMyNotificationsQuery(table), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return Ok(result);
    }
}
