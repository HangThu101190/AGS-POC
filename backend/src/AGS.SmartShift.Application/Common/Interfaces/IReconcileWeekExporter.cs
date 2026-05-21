using AGS.SmartShift.Application.Contracts.Reconcile;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IReconcileWeekExporter
{
    byte[] ExportWeek(ReconcileSummaryDto summary);
}
