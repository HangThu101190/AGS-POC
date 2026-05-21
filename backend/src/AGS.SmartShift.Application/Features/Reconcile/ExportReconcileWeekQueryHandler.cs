using AGS.SmartShift.Application.Common.Interfaces;
using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed class ExportReconcileWeekQueryHandler : IRequestHandler<ExportReconcileWeekQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly IReconcileWeekExporter _exporter;

    public ExportReconcileWeekQueryHandler(IMediator mediator, IReconcileWeekExporter exporter)
    {
        _mediator = mediator;
        _exporter = exporter;
    }

    public async Task<byte[]> Handle(ExportReconcileWeekQuery request, CancellationToken cancellationToken)
    {
        var summary = await _mediator.Send(
            new GetReconcileSummaryQuery(request.WeekId, request.DepartmentCode),
            cancellationToken);
        return _exporter.ExportWeek(summary);
    }
}
