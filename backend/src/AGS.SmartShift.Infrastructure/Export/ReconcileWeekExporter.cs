using AGS.SmartShift.Application.Contracts.Reconcile;
using AGS.SmartShift.Application.Planning;
using ClosedXML.Excel;

namespace AGS.SmartShift.Infrastructure.Export;

public static class ReconcileWeekExporter
{
    private const string TitleRed = "B91C1C";
    private const string HeaderBg = "F2E5BC";
    private const string HeaderFg = "1159C6";
    private const string NightShift = "50D092";
    private const string FullShift = "F0B000";
    private const string AfternoonShift = "00C0FF";
    private const string SpecialShift = "A03070";
    private const string Weekoff = "CCF2FF";
    private const string Holiday = "ADCBF8";

    public static byte[] ExportWeek(ReconcileSummaryDto summary)
    {
        using var wb = new XLWorkbook();
        var sheetName = summary.WeekId.Replace('/', '.').Replace(':', '-');
        var ws = wb.AddWorksheet(sheetName.Length > 31 ? sheetName[..31] : sheetName);

        var weekDates = summary.Employees.FirstOrDefault()?.Days.Select(d => d.DateLabel).ToList()
            ?? Enumerable.Range(0, 7).Select(i => $"D{i + 1}").ToList();

        ws.Range(1, 1, 1, 9).Merge();
        ws.Cell(1, 1).Value = $"TH CÔNG · {summary.DepartmentCode} · {summary.WeekId}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml($"#{TitleRed}");
        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var dayNames = new[] { "THỨ 2", "THỨ 3", "THỨ 4", "THỨ 5", "THỨ 6", "THỨ 7", "CN" };
        ws.Cell(2, 1).Value = "STT";
        ws.Cell(2, 2).Value = "TÊN NHÂN VIÊN";
        for (var d = 0; d < 7; d++)
        {
            ws.Cell(2, 3 + d).Value = $"{dayNames[d]}\n{weekDates.ElementAtOrDefault(d)}";
        }

        StyleHeaderRow(ws, 2, 9);

        var row = 3;
        var stt = 1;
        foreach (var emp in summary.Employees)
        {
            var segRow = row;
            var hourRow = row + 1;
            var weekTotal = 0.0;

            ws.Cell(segRow, 1).Value = stt;
            ws.Cell(segRow, 2).Value = emp.Name;
            for (var d = 0; d < 7; d++)
            {
                var day = emp.Days.FirstOrDefault(x => x.DayIdx == d);
                ws.Cell(segRow, 3 + d).Value = FormatSegCell(day);
                ws.Cell(segRow, 3 + d).Style.Fill.BackgroundColor = XLColor.FromHtml(
                    $"#{CellFillHex(day)}");
                ws.Cell(segRow, 3 + d).Style.Font.FontColor = XLColor.FromHtml(
                    $"#{SegFontHex(CellFillHex(day))}");
                weekTotal += day?.ActualTotalHours ?? day?.PlannedTotalHours ?? 0;
            }

            ws.Cell(hourRow, 2).Value = weekTotal;
            ws.Cell(hourRow, 2).Style.Font.Bold = true;
            for (var d = 0; d < 7; d++)
            {
                var day = emp.Days.FirstOrDefault(x => x.DayIdx == d);
                ws.Cell(hourRow, 3 + d).Value = day?.ActualCode ?? day?.PlannedCode ?? "";
            }

            StyleDataRow(ws, segRow, 9);
            StyleDataRow(ws, hourRow, 9);
            row += 2;
            stt++;
        }

        ws.SheetView.FreezeRows(2);
        ws.SheetView.FreezeColumns(2);
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static string FormatSegCell(ReconcileDayDto? day)
    {
        if (day is null)
        {
            return "";
        }

        if (day.CodeType == "weekoff")
        {
            return "T";
        }

        if (day.CodeType == "holiday")
        {
            return "L";
        }

        var segs = day.ActualSegments.Length > 0 ? day.ActualSegments : day.PlannedSegments;
        if (segs.Length == 0)
        {
            return day.ActualCode;
        }

        var text = string.Join(
            " / ",
            segs.Select(seg =>
            {
                var parts = seg.Split('-', 2);
                if (parts.Length != 2)
                {
                    return seg;
                }

                return $"{parts[0].PadLeft(2, '0')}h00-{parts[1].Replace("+", "").PadLeft(2, '0')}h00";
            }));

        return day.HasRevision ? $"{text} SĐ" : text;
    }

    private static string CellFillHex(ReconcileDayDto? day)
    {
        if (day is null)
        {
            return "FFFFFF";
        }

        if (day.CodeType == "weekoff")
        {
            return Weekoff;
        }

        if (day.CodeType == "holiday")
        {
            return Holiday;
        }

        var segs = day.ActualSegments.Length > 0 ? day.ActualSegments : day.PlannedSegments;
        if (segs.Length == 0)
        {
            return "FFFFFF";
        }

        if (segs.Length > 1)
        {
            return SpecialShift;
        }

        var result = AttendanceCodeEngine.FromSegments(segs, day.DateLabel);
        if (result.NightHours > result.DayHours)
        {
            return NightShift;
        }

        if (TryParseSegmentHours(segs[0], out var start, out _) && start >= 12)
        {
            return AfternoonShift;
        }

        return FullShift;
    }

    private static bool TryParseSegmentHours(string segment, out int startHour, out int endHour)
    {
        startHour = 0;
        endHour = 0;
        var parts = segment.Split('-', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        return int.TryParse(parts[0], out startHour) && int.TryParse(parts[1].Replace("+", ""), out endHour);
    }

    private static string SegFontHex(string bgHex) =>
        bgHex is "F0B000" or "50D092" or "A03070" ? "FFFFFF" : "0F172A";

    private static void StyleHeaderRow(IXLWorksheet ws, int r, int cols)
    {
        for (var c = 1; c <= cols; c++)
        {
            var cell = ws.Cell(r, c);
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.FromHtml($"#{HeaderFg}");
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml($"#{HeaderBg}");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }

    private static void StyleDataRow(IXLWorksheet ws, int r, int cols)
    {
        for (var c = 1; c <= cols; c++)
        {
            ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }
}
