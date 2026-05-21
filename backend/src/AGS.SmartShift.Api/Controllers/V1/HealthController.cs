using AGS.SmartShift.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(new
        {
            status = "healthy",
            service = "AGS.SmartShift.Api",
            requestLanguage = HttpContext.GetRequestLanguage(),
            utc = DateTime.UtcNow,
        });
}
