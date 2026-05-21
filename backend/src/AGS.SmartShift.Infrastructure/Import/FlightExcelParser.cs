using System.Globalization;
using System.Text.RegularExpressions;
using AGS.SmartShift.Application.Common.Interfaces;
using ClosedXML.Excel;

namespace AGS.SmartShift.Infrastructure.Import;

public sealed class FlightExcelParser : IFlightExcelParser
{
    private static readonly Regex DayTitleRegex = new(
        @"NG[ÀA]Y\s+(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Task<FlightExcelParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(content);
        var sheet = workbook.Worksheets.First();
        var used = sheet.RangeUsed();
        if (used == null)
        {
            return Task.FromResult(new FlightExcelParseResult());
        }

        // Read a full rectangular grid (preserve column indices). CellsUsed() skips blanks and
        // shifts values left — e.g. REGS (20 chars) can land in Aircraft (varchar 16).
        var rows = ReadSheetGrid(sheet, used);

        var warnings = new List<string>();
        var dayMeta = DetectDayFromTitle(rows);
        var headerIdx = FindHeaderRowIndex(rows);
        if (headerIdx < 0)
        {
            warnings.Add("Không tìm thấy dòng tiêu đề (ARR FLT NO / ROUTE / STA).");
            return Task.FromResult(new FlightExcelParseResult { Warnings = warnings });
        }

        var header = rows[headerIdx]!;
        var col = MapColumns(header);
        var parsedRows = new List<ParsedFlightRow>();
        var lastDataRowIdx = headerIdx;
        var sortOrder = 0;

        for (var i = headerIdx + 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null || row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var arrNo = Get(row, col.ArrNo) ?? Get(row, col.FltNo);
            if (string.IsNullOrWhiteSpace(arrNo) || IsFooterOrNoiseRow(arrNo, row, col))
            {
                continue;
            }

            var route = Get(row, col.Route) ?? "";
            var sta = NormalizeTime(Get(row, col.Sta) ?? "");
            var stdRaw = Get(row, col.Std) ?? "";
            var std = NormalizeTime(stdRaw);
            if (string.IsNullOrWhiteSpace(sta))
            {
                continue;
            }

            sortOrder++;
            lastDataRowIdx = i;
            var excelNo = ParseExcelRowNo(Get(row, col.No)) ?? sortOrder;
            var remark = Get(row, col.Remark) ?? "";
            var registration = Get(row, col.Regs);
            var aircraft = Get(row, col.Aircraft);
            var carry = Get(row, col.Carry);
            var eta = NormalizeTimeOptional(Get(row, col.Eta));
            var etd = NormalizeTimeOptional(Get(row, col.Etd));
            var dept = NormalizeDept(Get(row, col.Dept));
            var manning = ParseManning(Get(row, col.Manning)) ?? GuessManning(aircraft);

            parsedRows.Add(new ParsedFlightRow
            {
                ExcelRowNo = excelNo,
                SortOrder = sortOrder,
                FlightNo = Truncate(arrNo.Trim().ToUpperInvariant(), ImportFieldLimits.FlightNo),
                DepartureFlightNo = TruncateOptional(Get(row, col.DepNo)?.Trim().ToUpperInvariant(), ImportFieldLimits.FlightNo),
                Route = Truncate(route.Trim(), ImportFieldLimits.Route),
                Sta = Truncate(sta, ImportFieldLimits.Time),
                Std = Truncate(string.IsNullOrWhiteSpace(std) ? sta : std, ImportFieldLimits.Time),
                Eta = TruncateOptional(eta, ImportFieldLimits.Time),
                Etd = TruncateOptional(etd, ImportFieldLimits.Time),
                DepartmentCode = Truncate(dept, ImportFieldLimits.DepartmentCode),
                Manning = manning,
                ManningExplain = null,
                Registration = TruncateOptional(registration?.Trim().ToUpperInvariant(), ImportFieldLimits.Registration),
                Aircraft = TruncateOptional(aircraft, ImportFieldLimits.Aircraft),
                Carry = TruncateOptional(carry, ImportFieldLimits.Carry),
                IsVip = Regex.IsMatch(remark, @"vip", RegexOptions.IgnoreCase),
                VipNote = Regex.IsMatch(remark, @"vip", RegexOptions.IgnoreCase)
                    ? Truncate(remark, ImportFieldLimits.VipNote)
                    : null,
                Gate = TruncateOptional(Get(row, col.Gate), ImportFieldLimits.Gate),
                Belt = TruncateOptional(Get(row, col.Belt), ImportFieldLimits.Belt),
                Parking = TruncateOptional(Get(row, col.Prk), ImportFieldLimits.Parking),
                Remark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim(),
            });
        }

        var sheetRemark = NormalizeSheetText(ExtractSheetRemark(rows, lastDataRowIdx));
        var mergedRows = FlightFooterRemarkMerger.Merge(parsedRows, sheetRemark);

        return Task.FromResult(new FlightExcelParseResult
        {
            TargetDayIdx = null,
            TargetDayLabel = dayMeta,
            SheetRemark = sheetRemark,
            Rows = mergedRows,
            Warnings = warnings,
        });
    }

    private static string? DetectDayFromTitle(IReadOnlyList<string?[]> rows)
    {
        foreach (var row in rows.Take(8))
        {
            var text = string.Join(" ", row.Select(c => c ?? ""));
            var m = DayTitleRegex.Match(text);
            if (!m.Success)
            {
                continue;
            }

            var dd = m.Groups[1].Value.PadLeft(2, '0');
            var mm = m.Groups[2].Value.PadLeft(2, '0');
            return $"{dd}/{mm}";
        }

        return null;
    }

    private static int FindHeaderRowIndex(IReadOnlyList<string?[]> rows)
    {
        for (var i = 0; i < Math.Min(12, rows.Count); i++)
        {
            var joined = string.Join(" ", rows[i].Select(c => c ?? "")).ToUpperInvariant();
            if (joined.Contains("ARR FLT") || joined.Contains("FLT NO") || joined.Contains("ROUTE"))
            {
                return i;
            }
        }

        return -1;
    }

    private static ColumnMap MapColumns(string?[] header)
    {
        var map = new ColumnMap();
        for (var i = 0; i < header.Length; i++)
        {
            var h = (header[i] ?? "").Trim().ToUpperInvariant();
            if (h.Contains("ARR FLT"))
            {
                map.ArrNo = i;
            }
            else if (h is "FLT NO" or "FLIGHT NO")
            {
                map.FltNo = i;
            }
            else if (h.Contains("DEP FLT"))
            {
                map.DepNo = i;
            }
            else if (h.Contains("ROUTE"))
            {
                map.Route = i;
            }
            else if (h is "STA")
            {
                map.Sta = i;
            }
            else if (h is "STD")
            {
                map.Std = i;
            }
            else if (h is "ETA")
            {
                map.Eta = i;
            }
            else if (h is "ETD")
            {
                map.Etd = i;
            }
            else if (h.Contains("REMARK"))
            {
                map.Remark = i;
            }
            else if ((h.Contains("AIRCRAFT") || h is "AC") && !h.Contains("REG"))
            {
                map.Aircraft = i;
            }
            else if (h is "REGS" or "REG" or "REGISTRATION")
            {
                map.Regs = i;
            }
            else if (h.Contains("MANNING") || h.Contains("ĐỊNH BIÊN"))
            {
                map.Manning = i;
            }
            else if (h.Contains("DEPT") || h.Contains("PHÒNG"))
            {
                map.Dept = i;
            }
            else if (h is "GATE")
            {
                map.Gate = i;
            }
            else if (h.Contains("BELT"))
            {
                map.Belt = i;
            }
            else if (h is "PRK" || h.Contains("PARK"))
            {
                map.Prk = i;
            }
            else if (h is "CARRY")
            {
                map.Carry = i;
            }
            else if (h == "NO" || h == "STT" || h.StartsWith("NO ", StringComparison.Ordinal))
            {
                map.No = i;
            }
        }

        return map;
    }

    private static int? ParseExcelRowNo(string? value) =>
        int.TryParse(value?.Trim(), out var n) && n > 0 ? n : null;

    private static string? NormalizeSheetText(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? null
            : text.Replace("_x000D_", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');

    private static string? ExtractSheetRemark(IReadOnlyList<string?[]> rows, int lastDataRowIdx)
    {
        var lines = new List<string>();
        for (var i = lastDataRowIdx + 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null || row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var text = string.Join("  ", row.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim()));
            if (!string.IsNullOrWhiteSpace(text))
            {
                lines.Add(text);
            }
        }

        return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    private static List<string?[]> ReadSheetGrid(IXLWorksheet sheet, IXLRange used)
    {
        var firstRow = used.FirstRow().RowNumber();
        var lastRow = used.LastRow().RowNumber();
        var firstCol = used.FirstColumn().ColumnNumber();
        var lastCol = used.LastColumn().ColumnNumber();
        var width = lastCol - firstCol + 1;
        var rows = new List<string?[]>(lastRow - firstRow + 1);

        for (var r = firstRow; r <= lastRow; r++)
        {
            var cells = new string?[width];
            for (var c = firstCol; c <= lastCol; c++)
            {
                cells[c - firstCol] = ReadCellText(sheet.Cell(r, c));
            }

            rows.Add(cells);
        }

        return rows;
    }

    private static bool IsFooterOrNoiseRow(string arrNo, string?[] row, ColumnMap col)
    {
        var key = arrNo.Trim();
        if (key.Length > ImportFieldLimits.FlightNo)
        {
            return true;
        }

        if (key.Equals("remark", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (key.StartsWith("trong ke hoach", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith('*'))
        {
            return true;
        }

        var hasSignal =
            !string.IsNullOrWhiteSpace(Get(row, col.ArrNo))
            || !string.IsNullOrWhiteSpace(Get(row, col.DepNo))
            || !string.IsNullOrWhiteSpace(Get(row, col.Route));

        return !hasSignal;
    }

    private static string? Get(string?[] row, int? idx)
    {
        if (idx is not int i || i < 0 || i >= row.Length)
        {
            return null;
        }

        var value = row[i];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ReadCellText(IXLCell cell)
    {
        if (cell.DataType == XLDataType.DateTime)
        {
            return cell.GetDateTime().ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        if (cell.DataType == XLDataType.TimeSpan)
        {
            return cell.GetTimeSpan().ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }

        return cell.GetString();
    }

    private static string NormalizeTime(string value)
    {
        var v = value.Trim();
        if (string.IsNullOrEmpty(v))
        {
            return v;
        }

        var hasPlus = v.Contains('+', StringComparison.Ordinal);
        var core = hasPlus ? v.Replace("+", "", StringComparison.Ordinal).Trim() : v;

        if (DateTime.TryParse(core, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            || DateTime.TryParse(core, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
        {
            var formatted = dt.ToString("HH:mm", CultureInfo.InvariantCulture);
            return hasPlus ? $"{formatted}+" : formatted;
        }

        var match = Regex.Match(core, @"(\d{1,2}):(\d{2})");
        if (match.Success)
        {
            var formatted = $"{match.Groups[1].Value.PadLeft(2, '0')}:{match.Groups[2].Value}";
            return hasPlus ? $"{formatted}+" : formatted;
        }

        if (core.Length == 4 && int.TryParse(core, out _))
        {
            var formatted = $"{core[..2]}:{core[2..]}";
            return hasPlus ? $"{formatted}+" : formatted;
        }

        return Truncate(v, ImportFieldLimits.Time);
    }

    private static string? NormalizeTimeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : NormalizeTime(value);

    private static string NormalizeDept(string? value)
    {
        var dept = string.IsNullOrWhiteSpace(value) ? "PVHK_DI" : value.Trim().ToUpperInvariant();
        return Truncate(dept, ImportFieldLimits.DepartmentCode);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private static string? TruncateOptional(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : Truncate(value.Trim(), max);

    private static int? ParseManning(string? value) =>
        int.TryParse(value, out var n) ? n : null;

    private static int GuessManning(string? aircraft) =>
        aircraft switch
        {
            "359" => 5,
            "321" => 3,
            "320" => 3,
            _ => 3,
        };

    private static class ImportFieldLimits
    {
        public const int FlightNo = 32;
        public const int Route = 64;
        public const int Time = 8;
        public const int DepartmentCode = 32;
        public const int Aircraft = 16;
        public const int Registration = 32;
        public const int Carry = 32;
        public const int ManningExplain = 512;
        public const int VipNote = 256;
        public const int Gate = 32;
        public const int Belt = 32;
        public const int Parking = 32;
    }

    private sealed class ColumnMap
    {
        public int? No { get; set; }
        public int? ArrNo { get; set; }
        public int? FltNo { get; set; }
        public int? DepNo { get; set; }
        public int? Route { get; set; }
        public int? Sta { get; set; }
        public int? Std { get; set; }
        public int? Eta { get; set; }
        public int? Etd { get; set; }
        public int? Remark { get; set; }
        public int? Carry { get; set; }
        public int? Aircraft { get; set; }
        public int? Manning { get; set; }
        public int? Dept { get; set; }
        public int? Gate { get; set; }
        public int? Belt { get; set; }
        public int? Prk { get; set; }
        public int? Regs { get; set; }
    }
}
