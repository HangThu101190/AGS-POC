using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Reconcile;

namespace AGS.SmartShift.Infrastructure.Export;

public sealed class ReconcileMonthExporterService : IReconcileMonthExporter
{
    public byte[] ExportMonth(ReconcileSummaryDto summary, string monthLabel) =>
        ReconcileMonthExporter.ExportMonth(summary, monthLabel);
}
