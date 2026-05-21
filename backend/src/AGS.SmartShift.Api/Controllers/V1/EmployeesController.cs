using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Application.Features.Identity.Employees;
using CreateEmployeeUserCommand = AGS.SmartShift.Application.Features.Identity.Employees.CreateEmployeeUserCommand;
using PatchEmployeeUserCommand = AGS.SmartShift.Application.Features.Identity.Employees.PatchEmployeeUserCommand;
using ResetEmployeePasswordCommand = AGS.SmartShift.Application.Features.Identity.Employees.ResetEmployeePasswordCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] TableListRequest table,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListEmployeesQuery(table), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] EmployeeUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateEmployeeCommand(body), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] EmployeeUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateEmployeeCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateEmployeeCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/user")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUser(
        Guid id,
        [FromBody] CreateEmployeeUserRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateEmployeeUserCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetEmployeePasswordCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/user")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchUser(
        Guid id,
        [FromBody] PatchEmployeeUserRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PatchEmployeeUserCommand(id, body), cancellationToken);
        return Ok(result);
    }
}
