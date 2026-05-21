using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Assignments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize(Policy = SmartShiftRoles.PolicySup)]
[Route("api/v1/assignments")]
public sealed class AssignmentsController : ControllerBase
{
    private readonly ISender _sender;

    public AssignmentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignToSlotRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignToSlotCommand(body.WeekId, body.DepartmentId, body.SlotId, body.EmployeeId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{assignmentId:guid}/flights")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LinkFlights(
        Guid assignmentId,
        [FromBody] LinkAssignmentFlightsRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LinkAssignmentFlightsCommand(body.WeekId, assignmentId, body.FlightNos ?? []),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        Guid assignmentId,
        [FromQuery] string weekId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoveAssignmentCommand(weekId, assignmentId), cancellationToken);
        return NoContent();
    }
}

public sealed class AssignToSlotRequest
{
    public string? WeekId { get; init; }
    public Guid DepartmentId { get; init; }
    public Guid SlotId { get; init; }
    public Guid EmployeeId { get; init; }
}

public sealed class LinkAssignmentFlightsRequest
{
    public string? WeekId { get; init; }
    public IReadOnlyList<string>? FlightNos { get; init; }
}
