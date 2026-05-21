using AGS.SmartShift.Application.Contracts.Reconcile;
using AGS.SmartShift.Application.Planning;
using ClosedXML.Excel;

namespace AGS.SmartShift.Infrastructure.Export;

/// <summary>Monthly TH CONG aggregate (week slice) — mirrors prototype exportMonthTHCong structure.</summary>
public static class ReconcileMonthExporter
{
    public static byte[] ExportMonth(ReconcileSummaryDto summary, string monthLabel)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("TH CONG");

        ws.Cell(1, 1).Value = $"BẢNG CHẤM CÔNG THÁNG — {monthLabel}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 10).Merge();

        var headers = new[]
        {
            "STT", "Mã NV", "Tên NV", "N", "D", "L", "T", "F", "NB", "Tổng giờ",
        };
        for (var c = 0; c < headers.Length; c++)
        {
            ws.Cell(3, c + 1).Value = headers[c];
            ws.Cell(3, c + 1).Style.Font.Bold = true;
            ws.Cell(3, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
        }

        var row = 4;
        var stt = 1;
        foreach (var emp in summary.Employees)
        {
            var agg = AggregateEmployee(emp);
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = emp.Code;
            ws.Cell(row, 3).Value = emp.Name;
            ws.Cell(row, 4).Value = agg.DayHours;
            ws.Cell(row, 5).Value = agg.NightHours;
            ws.Cell(row, 6).Value = agg.HolidayDays;
            ws.Cell(row, 7).Value = agg.WeekoffDays;
            ws.Cell(row, 8).Value = agg.LeaveF;
            ws.Cell(row, 9).Value = agg.LeaveNb;
            ws.Cell(row, 10).Value = agg.TotalHours;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static MonthAgg AggregateEmployee(ReconcileEmployeeDto emp)
    {
        var agg = new MonthAgg();
        foreach (var day in emp.Days)
        {
            var code = AttendanceCodeEngine.FromSegments(
                day.ActualSegments.Length > 0 ? day.ActualSegments : day.PlannedSegments,
                day.DateLabel);

            switch (code.Type)
            {
                case "holiday":
                    agg.HolidayDays++;
                    agg.DayHours += code.DayHours;
                    agg.NightHours += code.NightHours;
                    break;
                case "weekoff":
                    agg.WeekoffDays++;
                    break;
                case "leave":
                    if (code.Code == "F")
                    {
                        agg.LeaveF++;
                    }
                    else if (code.Code == "NB")
                    {
                        agg.LeaveNb++;
                    }

                    break;
                default:
                    agg.DayHours += code.DayHours;
                    agg.NightHours += code.NightHours;
                    break;
            }
        }

        agg.TotalHours = agg.DayHours + agg.NightHours;
        return agg;
    }

    private sealed class MonthAgg
    {
        public double DayHours { get; set; }
        public double NightHours { get; set; }
        public int HolidayDays { get; set; }
        public int WeekoffDays { get; set; }
        public int LeaveF { get; set; }
        public int LeaveNb { get; set; }
        public double TotalHours { get; set; }
    }
}
