using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Attendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/attendance")]
public sealed class AttendanceController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceController(ISender sender) => _sender = sender;

    [HttpGet("my-shift")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTodayShift(
        [FromQuery] string? weekId,
        CancellationToken cancellationToken)
    {
        var shift = await _sender.Send(new GetMyTodayShiftQuery(weekId), cancellationToken);
        return Ok(shift);
    }

    [HttpGet("{employeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var record = await _sender.Send(new GetAttendanceByEmployeeQuery(employeeId), cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost("check-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest body,
        CancellationToken cancellationToken)
    {
        var record = await _sender.Send(
            new CheckInCommand(body.Lat, body.Lng, body.InZone, body.GeoNote, body.GeoSimulated),
            cancellationToken);
        return Ok(record);
    }

    [HttpPost("location-samples")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostLocationSamples(
        [FromBody] LocationSamplesRequest body,
        CancellationToken cancellationToken)
    {
        var samples = (body.Samples ?? [])
            .Select(s => new LocationSampleInput
            {
                Lat = s.Lat,
                Lng = s.Lng,
                CapturedAtUtc = s.CapturedAtUtc,
            })
            .ToList();

        var result = await _sender.Send(new PostLocationSamplesCommand(samples), cancellationToken);
        return Ok(result);
    }

    [HttpPost("check-out")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutRequest? body,
        CancellationToken cancellationToken)
    {
        var record = await _sender.Send(
            new CheckOutCommand(body?.EarlyNote),
            cancellationToken);
        return Ok(record);
    }

    public sealed class CheckInRequest
    {
        public double Lat { get; init; }
        public double Lng { get; init; }
        public bool InZone { get; init; }
        public string? GeoNote { get; init; }
        public bool GeoSimulated { get; init; }
    }

    public sealed class CheckOutRequest
    {
        public string? EarlyNote { get; init; }
    }

    public sealed class LocationSamplesRequest
    {
        public IReadOnlyList<LocationSampleRequest>? Samples { get; init; }
    }

    public sealed class LocationSampleRequest
    {
        public double Lat { get; init; }
        public double Lng { get; init; }
        public DateTime? CapturedAtUtc { get; init; }
    }
}
