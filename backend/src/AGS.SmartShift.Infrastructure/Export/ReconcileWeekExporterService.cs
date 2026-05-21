using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Reconcile;

namespace AGS.SmartShift.Infrastructure.Export;

public sealed class ReconcileWeekExporterService : IReconcileWeekExporter
{
    public byte[] ExportWeek(ReconcileSummaryDto summary) => ReconcileWeekExporter.ExportWeek(summary);
}
