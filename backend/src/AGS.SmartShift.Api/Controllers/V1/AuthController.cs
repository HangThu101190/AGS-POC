using System.Security.Claims;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Application.Features.Auth.Login;
using AGS.SmartShift.Application.Features.Auth.Logout;
using AGS.SmartShift.Application.Features.Auth.Me;
using AGS.SmartShift.Application.Features.Auth.Refresh;
using ChangePasswordCommand = AGS.SmartShift.Application.Features.Auth.Me.ChangePasswordCommand;
using UpdateMeCommand = AGS.SmartShift.Application.Features.Auth.Me.UpdateMeCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request), cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefreshTokenCommand(request), cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new LogoutCommand(request), cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!TryGetUserAccountId(out var accountId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new GetCurrentUserQuery(accountId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPatch("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateMeRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserAccountId(out var accountId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new UpdateMeCommand(accountId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("me/change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserAccountId(out var accountId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new ChangePasswordCommand(accountId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    private bool TryGetUserAccountId(out Guid accountId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out accountId);
    }
}
