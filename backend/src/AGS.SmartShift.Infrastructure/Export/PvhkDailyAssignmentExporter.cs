using System.Reflection;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Enums;
using ClosedXML.Excel;

namespace AGS.SmartShift.Infrastructure.Export;

public sealed class PvhkDailyAssignmentExporter : IPvhkDailyAssignmentExporter
{
    private const string TemplateResourceName = "Seed.Data.Pvhk_Di_Lich_phan_cong_theo_ngay.xlsx";
    private const int DataStartRow = 10;
    private const int MaxDataRows = 55;

    public byte[] Export(PvhkExportRequest request)
    {
        using var templateStream = OpenTemplateStream();
        using var workbook = new XLWorkbook(templateStream);
        var sheet = workbook.Worksheets.First();
        while (workbook.Worksheets.Count > 1)
        {
            workbook.Worksheet(workbook.Worksheets.Count).Delete();
        }

        sheet.Name = SanitizeSheetName(request.DayLabel);
        FillDaySheet(sheet, request);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportWeek(PvhkWeekExportRequest request)
    {
        if (request.Days.Count == 0)
        {
            throw new InvalidOperationException("Week export requires at least one day.");
        }

        using var templateStream = OpenTemplateStream();
        using var templateBook = new XLWorkbook(templateStream);
        var prototype = templateBook.Worksheets.First();

        using var workbook = new XLWorkbook();
        foreach (var day in request.Days)
        {
            var sheet = prototype.CopyTo(workbook, SanitizeSheetName(day.DayLabel));
            FillDaySheet(sheet, day);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void FillDaySheet(IXLWorksheet sheet, PvhkExportRequest request)
    {
        FillHeaderRows(sheet, request);
        ClearDataRows(sheet);

        var qnRow = DataStartRow;
        var qnStt = 1;
        var qnLines = request.Day.Lines
            .Where(l => l.Segment == OperationalSegment.Qn)
            .OrderBy(l => l.SortOrder)
            .ToList();

        if (qnLines.Count > 0)
        {
            foreach (var line in qnLines)
            {
                WriteQnRow(sheet, qnRow++, qnStt++, line, request.Day);
            }
        }
        else
        {
            foreach (var flight in request.Day.Flights.OrderBy(f => f.Std))
            {
                WriteQnFlightOnlyRow(sheet, qnRow++, qnStt++, flight);
            }
        }

        var qtRow = DataStartRow;
        var qtStt = 1;
        foreach (var line in request.Day.Lines
                     .Where(l => l.Segment == OperationalSegment.Qt)
                     .OrderBy(l => l.SortOrder))
        {
            WriteQtRow(sheet, qtRow++, qtStt++, line, request.Day);
        }

        var gioStart = Math.Max(qnRow, qtRow) + 2;
        WriteGioLenCaSection(sheet, gioStart, request.Day);
    }

    private static void FillHeaderRows(IXLWorksheet sheet, PvhkExportRequest request)
    {
        sheet.Cell(1, 1).Value = "CONG TY CO PHAN PVMD AGS CAM RANH";
        sheet.Cell(2, 1).Value = "Phong Phuc vu hanh khach Di";
        var assigner = request.Bio?.AssignerName ?? string.Empty;
        sheet.Cell(3, 1).Value = string.IsNullOrWhiteSpace(assigner)
            ? "Nguoi phan cong:"
            : $"Nguoi phan cong: {assigner}";
        sheet.Cell(3, 8).Value = $"Ngay: {request.DayLabel}/{request.CalendarYear}";
        sheet.Cell(4, 1).Value = FormatScheduleTitle(request.DayLabel, request.CalendarYear);
        var bioParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Bio?.MorningSupName))
        {
            bioParts.Add($"HC-S: {request.Bio.MorningSupName}");
        }

        if (!string.IsNullOrWhiteSpace(request.Bio?.EveningSupName))
        {
            bioParts.Add($"C-D: {request.Bio.EveningSupName}");
        }

        if (!string.IsNullOrWhiteSpace(request.Bio?.RadioNote))
        {
            bioParts.Add(request.Bio.RadioNote);
        }

        sheet.Cell(5, 1).Value = bioParts.Count > 0 ? string.Join(" | ", bioParts) : string.Empty;
    }

    private static void WriteGioLenCaSection(IXLWorksheet sheet, int startRow, StaffingDayDto day)
    {
        sheet.Cell(startRow, 1).Value = "GIO LEN CA";
        var row = startRow + 1;
        sheet.Cell(row, 1).Value = "Gio lam viec";
        sheet.Cell(row, 4).Value = "Ghi chu";
        sheet.Cell(row, 7).Value = "Nhan su";
        row++;

        foreach (var segment in new[] { OperationalSegment.Qn, OperationalSegment.Qt })
        {
            var segmentLabel = segment == OperationalSegment.Qn ? "QN" : "QT";
            var groups = day.Assignments
                .Where(a => day.Lines.Any(l => l.Id == a.StaffingLineId && l.Segment == segment))
                .GroupBy(a => $"{a.WorkStart}-{a.WorkEnd}")
                .OrderBy(g => g.Key);
            foreach (var group in groups)
            {
                var names = string.Join(", ", group.Select(a => ShortName(a.EmployeeName)).Distinct());
                sheet.Cell(row, 1).Value = FormatShiftWindow(group.Key);
                sheet.Cell(row, 4).Value = segmentLabel;
                sheet.Cell(row, 7).Value = names;
                row++;
            }
        }
    }

    private static string FormatShiftWindow(string workWindow)
    {
        var parts = workWindow.Split('-', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return workWindow;
        }

        return $"{FormatCompactTime(parts[0])}-{FormatCompactTime(parts[1])}";
    }

    private static string FormatCompactTime(string hhmm)
    {
        if (!TimeOnly.TryParse(hhmm, out var time))
        {
            return hhmm;
        }

        return $"{time.Hour}H{time.Minute:00}";
    }

    private static string ShortName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0 ? fullName : parts[^1].ToUpperInvariant();
    }

    private static Stream OpenTemplateStream()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(TemplateResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Missing embedded template: {TemplateResourceName}");
        }

        return stream;
    }

    private static void ClearDataRows(IXLWorksheet sheet)
    {
        for (var row = DataStartRow; row < DataStartRow + MaxDataRows; row++)
        {
            for (var col = 1; col <= 15; col++)
            {
                sheet.Cell(row, col).Value = Blank.Value;
            }
        }
    }

    private static void WriteQnRow(
        IXLWorksheet sheet,
        int row,
        int stt,
        StaffingLineDto line,
        StaffingDayDto day)
    {
        var flight = day.Flights.FirstOrDefault(f => f.Id == line.FlightId);
        if (flight is null)
        {
            return;
        }

        var assigns = day.Assignments.Where(a => a.StaffingLineId == line.Id).ToList();
        sheet.Cell(row, 1).Value = stt;
        sheet.Cell(row, 2).Value = flight.DepartureFlightNo ?? flight.FlightNo;
        sheet.Cell(row, 3).Value = DestFromRoute(flight.Route);
        sheet.Cell(row, 4).Value = FormatAircraft(flight.Aircraft);
        sheet.Cell(row, 5).Value = FormatEtd(flight);
        sheet.Cell(row, 6).Value = NameForRole(assigns, CrewRole.Counter);
        sheet.Cell(row, 7).Value = NameForRole(assigns, CrewRole.Gate);
    }

    private static void WriteQnFlightOnlyRow(IXLWorksheet sheet, int row, int stt, StaffingFlightDto flight)
    {
        sheet.Cell(row, 1).Value = stt;
        sheet.Cell(row, 2).Value = flight.DepartureFlightNo ?? flight.FlightNo;
        sheet.Cell(row, 3).Value = DestFromRoute(flight.Route);
        sheet.Cell(row, 4).Value = FormatAircraft(flight.Aircraft);
        sheet.Cell(row, 5).Value = FormatEtd(flight);
        sheet.Cell(row, 6).Value = string.Empty;
        sheet.Cell(row, 7).Value = string.Empty;
    }

    private static void WriteQtRow(
        IXLWorksheet sheet,
        int row,
        int stt,
        StaffingLineDto line,
        StaffingDayDto day)
    {
        var flight = day.Flights.FirstOrDefault(f => f.Id == line.FlightId);
        if (flight is null)
        {
            return;
        }

        var assigns = day.Assignments.Where(a => a.StaffingLineId == line.Id).ToList();
        sheet.Cell(row, 8).Value = stt;
        sheet.Cell(row, 9).Value = flight.DepartureFlightNo ?? flight.FlightNo;
        sheet.Cell(row, 10).Value = DestFromRoute(flight.Route);
        sheet.Cell(row, 11).Value = FormatEtd(flight);
        sheet.Cell(row, 12).Value = NameForRole(assigns, CrewRole.Sup);
        sheet.Cell(row, 13).Value = assigns.Count > 0 ? assigns.Count.ToString() : string.Empty;
    }

    private static string FormatEtd(StaffingFlightDto flight)
    {
        var etd = string.IsNullOrWhiteSpace(flight.Etd) ? flight.Std : flight.Etd!;
        if (flight.IsDelayed || flight.EtdDelayMinutes > 0 || flight.DelayMinutes > 0)
        {
            return $"{etd} dc";
        }

        return etd;
    }

    private static string FormatScheduleTitle(string dayLabel, int calendarYear)
    {
        var parts = dayLabel.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            return $"LỊCH PHÂN CÔNG NGÀY  {parts[0]}/ {parts[1]} / {calendarYear}";
        }

        return $"LỊCH PHÂN CÔNG NGÀY  {dayLabel}";
    }

    private static string SanitizeSheetName(string dayLabel)
    {
        var name = dayLabel.Replace('/', '.').Trim();
        if (name.Length == 0)
        {
            name = "Phan_cong";
        }

        if (name.Length > 31)
        {
            name = name[..31];
        }

        return name;
    }

    private static string DestFromRoute(string route)
    {
        var parts = route.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 2 ? parts[^1] : route;
    }

    private static string FormatAircraft(string? aircraft)
    {
        if (string.IsNullOrWhiteSpace(aircraft))
        {
            return string.Empty;
        }

        var a = aircraft.Trim().ToUpperInvariant();
        return a.StartsWith('A') ? a : $"A{a}";
    }

    private static string NameForRole(IReadOnlyList<StaffingAssignmentDto> assigns, CrewRole role)
    {
        var match = assigns.FirstOrDefault(a => a.Role == role);
        return match?.EmployeeName ?? string.Empty;
    }
}
