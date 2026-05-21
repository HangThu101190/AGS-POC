using AGS.SmartShift.Application.Features.Identity.Sites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class SitesController : ControllerBase
{
    private readonly ISender _sender;

    public SitesController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListSitesQuery(), cancellationToken);
        return Ok(result);
    }
}
