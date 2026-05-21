using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Features.Reconcile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGS.SmartShift.Api.Controllers.V1;

[ApiController]
[Authorize(Policy = SmartShiftRoles.PolicyHr)]
[Route("api/v1/reconcile")]
public sealed class ReconcileController : ControllerBase
{
    private readonly ISender _sender;

    public ReconcileController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(
        [FromQuery] string? weekId,
        [FromQuery] string departmentCode = "PVHK_DI",
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetReconcileSummaryQuery(weekId, departmentCode),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("export/week")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportWeek(
        [FromQuery] string? weekId,
        [FromQuery] string departmentCode = "PVHK_DI",
        CancellationToken cancellationToken = default)
    {
        var bytes = await _sender.Send(
            new ExportReconcileWeekQuery(weekId, departmentCode),
            cancellationToken);
        var fileName = $"TH_CONG_{departmentCode}_{weekId ?? "week"}.xlsx";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("export/month")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMonth(
        [FromQuery] string? weekId,
        [FromQuery] string departmentCode = "PVHK_DI",
        [FromQuery] string? monthLabel = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _sender.Send(
            new ExportReconcileMonthQuery(weekId, departmentCode, monthLabel),
            cancellationToken);
        var label = monthLabel ?? DateTime.UtcNow.ToString("yyyy-MM");
        var fileName = $"TH_CONG_THANG_{departmentCode}_{label}.xlsx";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
