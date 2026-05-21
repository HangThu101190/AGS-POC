using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Application.Features.Identity.Departments;
using DeactivateDepartmentCommand = AGS.SmartShift.Application.Features.Identity.Departments.DeactivateDepartmentCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public DepartmentsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] TableListRequest table,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListDepartmentsQuery(table), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] DepartmentUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateDepartmentCommand(body), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] DepartmentUpsertDto body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateDepartmentCommand(id, body), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = SmartShiftRoles.PolicyHr)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateDepartmentCommand(id), cancellationToken);
        return Ok(result);
    }
}
