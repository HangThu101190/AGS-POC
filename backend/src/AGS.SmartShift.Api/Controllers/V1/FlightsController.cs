using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Api.Security;
using AGS.SmartShift.Application.Features.Planning.Flights;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class FlightsController : ControllerBase
{
    private readonly ISender _sender;

    public FlightsController(ISender sender) => _sender = sender;

    [HttpGet("day-meta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDayMeta(
        [FromQuery] string? weekId,
        [FromQuery] int dayIdx,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFlightScheduleDayQuery(weekId, dayIdx), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? weekId,
        [FromQuery] int? dayIdx,
        [FromQuery] string? departmentCode,
        [FromQuery] TableListRequest table,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ListFlightsQuery(weekId, dayIdx, departmentCode, table),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] FlightUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateFlightCommand(body), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] FlightUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateFlightCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFlightCommand(id), cancellationToken);
        return NoContent();
    }

    private const int AsyncImportThresholdBytes = 2 * 1024 * 1024;

    [HttpPost("import")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(typeof(FlightImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FlightImportJobDto), StatusCodes.Status202Accepted)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Import(
        IFormFile file,
        [FromQuery] string? weekId,
        [FromQuery] int? dayIdx,
        CancellationToken cancellationToken)
    {
        if (!ExcelUploadValidator.TryValidate(file, out var validationError))
        {
            return BadRequest(new { title = validationError });
        }

        var preferAsync = Request.Headers.TryGetValue("Prefer", out var prefer)
            && prefer.ToString().Contains("respond-async", StringComparison.OrdinalIgnoreCase);

        if (file.Length > AsyncImportThresholdBytes || preferAsync)
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            var job = await _sender.Send(
                new EnqueueFlightImportCommand(buffer.ToArray(), weekId, dayIdx),
                cancellationToken).ConfigureAwait(false);
            return AcceptedAtAction(nameof(GetImportJob), new { id = job.Id }, job);
        }

        await using var stream = file.OpenReadStream();
        var result = await _sender.Send(new ImportFlightsCommand(stream, weekId, dayIdx), cancellationToken);
        return Ok(result);
    }

    [HttpGet("import/jobs/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(typeof(FlightImportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImportJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await _sender.Send(new GetFlightImportJobQuery(id), cancellationToken).ConfigureAwait(false);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPatch("{id:guid}/delay")]
    [Authorize(Policy = SmartShiftRoles.PolicyTbdh)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDelay(
        Guid id,
        [FromBody] SetFlightDelayRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SetFlightDelayCommand(id, body.DelayMinutes, body.EtaDelayMinutes, body.EtdDelayMinutes),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Class (not positional record) so camelCase JSON binds reliably.</summary>
    public sealed class SetFlightDelayRequest
    {
        public int? DelayMinutes { get; set; }
        public int? EtaDelayMinutes { get; set; }
        public int? EtdDelayMinutes { get; set; }
    }
}
