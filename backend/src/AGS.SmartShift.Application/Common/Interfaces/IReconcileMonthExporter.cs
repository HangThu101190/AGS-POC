using AGS.SmartShift.Application.Contracts.Reconcile;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IReconcileMonthExporter
{
    byte[] ExportMonth(ReconcileSummaryDto summary, string monthLabel);
}
