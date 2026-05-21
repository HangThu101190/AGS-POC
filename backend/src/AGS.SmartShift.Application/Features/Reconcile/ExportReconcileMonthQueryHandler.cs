using AGS.SmartShift.Application.Common.Interfaces;
using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed class ExportReconcileMonthQueryHandler : IRequestHandler<ExportReconcileMonthQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly IReconcileMonthExporter _exporter;

    public ExportReconcileMonthQueryHandler(IMediator mediator, IReconcileMonthExporter exporter)
    {
        _mediator = mediator;
        _exporter = exporter;
    }

    public async Task<byte[]> Handle(ExportReconcileMonthQuery request, CancellationToken cancellationToken)
    {
        var summary = await _mediator.Send(
            new GetReconcileSummaryQuery(request.WeekId, request.DepartmentCode),
            cancellationToken);

        var label = string.IsNullOrWhiteSpace(request.MonthLabel)
            ? DateTime.UtcNow.ToString("MM/yyyy")
            : request.MonthLabel.Trim();

        return _exporter.ExportMonth(summary, label);
    }
}
