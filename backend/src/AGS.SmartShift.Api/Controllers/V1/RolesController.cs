using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Application.Features.Identity.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender) => _sender = sender;

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(RoleCatalogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoleCatalogQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("catalog/permissions")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(typeof(RoleCatalogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCatalogPermissions(
        [FromBody] UpdateRoleCatalogPermissionsRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateRoleCatalogPermissionsCommand(body), cancellationToken);
        return Ok(result);
    }
}
