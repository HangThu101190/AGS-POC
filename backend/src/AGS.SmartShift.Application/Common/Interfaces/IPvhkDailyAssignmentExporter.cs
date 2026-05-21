using AGS.SmartShift.Application.Contracts.Staffing;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IPvhkDailyAssignmentExporter
{
    byte[] Export(PvhkExportRequest request);

    byte[] ExportWeek(PvhkWeekExportRequest request);
}
