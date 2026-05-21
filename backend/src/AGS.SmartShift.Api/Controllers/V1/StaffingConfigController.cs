using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Features.Staffing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/staffing/config")]
public sealed class StaffingConfigController : ControllerBase
{
    private readonly ISender _sender;

    public StaffingConfigController(ISender sender) => _sender = sender;

    [HttpGet("aircraft-manning-rules")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> ListAircraftRules(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListAircraftManningRulesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("aircraft-manning-rules")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> CreateAircraftRule(
        [FromBody] UpsertAircraftManningRuleDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpsertAircraftManningRuleCommand(null, body), cancellationToken);
        return Ok(result);
    }

    [HttpPut("aircraft-manning-rules/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> UpdateAircraftRule(
        Guid id,
        [FromBody] UpsertAircraftManningRuleDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpsertAircraftManningRuleCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("aircraft-manning-rules/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> DeleteAircraftRule(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAircraftManningRuleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("airline-manning-rules")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> ListAirlineRules(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListAirlineManningRulesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("airline-manning-rules")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> CreateAirlineRule(
        [FromBody] UpsertAirlineManningRuleDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpsertAirlineManningRuleCommand(null, body), cancellationToken);
        return Ok(result);
    }

    [HttpPut("airline-manning-rules/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> UpdateAirlineRule(
        Guid id,
        [FromBody] UpsertAirlineManningRuleDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpsertAirlineManningRuleCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("airline-manning-rules/{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> DeleteAirlineRule(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAirlineManningRuleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("shift-check-in-policies")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    public async Task<IActionResult> ListShiftPolicies(
        [FromQuery] string? departmentCode,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListShiftCheckInPoliciesQuery(departmentCode), cancellationToken);
        return Ok(result);
    }
}
