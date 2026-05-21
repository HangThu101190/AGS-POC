using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Features.Staffing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/staffing")]
public sealed class StaffingController : ControllerBase
{
    private readonly ISender _sender;

    public StaffingController(ISender sender) => _sender = sender;

    [HttpGet("days/{weekId}/{dayIdx:int}")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingView)]
    public async Task<IActionResult> GetDay(
        string weekId,
        int dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStaffingDayQuery(weekId, dayIdx, departmentCode), cancellationToken);
        return Ok(result);
    }

    [HttpGet("days/{weekId}/{dayIdx:int}/roster")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingView)]
    public async Task<IActionResult> GetRoster(
        string weekId,
        int dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStaffingRosterQuery(weekId, dayIdx, departmentCode), cancellationToken);
        return Ok(result);
    }

    [HttpPost("days/{weekId}/{dayIdx:int}/propose")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingTbdh)]
    public async Task<IActionResult> Propose(
        string weekId,
        int dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ProposeStaffingDayCommand(weekId, dayIdx, departmentCode), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("lines/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingTbdh)]
    public async Task<IActionResult> PatchLine(
        Guid id,
        [FromBody] PatchStaffingLineDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PatchStaffingLineCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpPost("assignments")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingAssign)]
    public async Task<IActionResult> CreateAssignment(
        [FromBody] CreateStaffingAssignmentDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateStaffingAssignmentCommand(body), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("assignments/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingAssign)]
    public async Task<IActionResult> PatchAssignment(
        Guid id,
        [FromBody] UpdateStaffingAssignmentDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateStaffingAssignmentCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("assignments/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingAssign)]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteStaffingAssignmentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("days/{weekId}/{dayIdx:int}/confirm")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingAssign)]
    public async Task<IActionResult> Confirm(
        string weekId,
        int dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConfirmStaffingDayCommand(weekId, dayIdx, departmentCode), cancellationToken);
        return Ok(result);
    }

    [HttpGet("days/{weekId}/{dayIdx:int}/export")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingView)]
    public async Task<IActionResult> Export(
        string weekId,
        int dayIdx,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var bytes = await _sender.Send(new ExportStaffingDayQuery(weekId, dayIdx, departmentCode), cancellationToken);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PVHK_{weekId}_day{dayIdx}.xlsx");
    }

    [HttpGet("weeks/{weekId}/export")]
    [Authorize(Policy = SmartShiftRoles.PolicyStaffingView)]
    public async Task<IActionResult> ExportWeek(
        string weekId,
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var bytes = await _sender.Send(new ExportStaffingWeekQuery(weekId, departmentCode), cancellationToken);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PVHK_Di_{weekId}.xlsx");
    }
}
