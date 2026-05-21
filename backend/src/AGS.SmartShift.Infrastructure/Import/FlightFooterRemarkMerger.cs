using System.Text;
using System.Text.RegularExpressions;
using AGS.SmartShift.Application.Common.Interfaces;

namespace AGS.SmartShift.Infrastructure.Import;

/// <summary>Maps VIP/CIP lines from the Excel footer Remark block onto flight rows.</summary>
internal static class FlightFooterRemarkMerger
{
    private static readonly Regex VipLineRegex = new(
        @"^\s*-\s*([A-Z0-9]{2,8})\s+([A-Z]{6,12})\s+(\d{3,4})\s+(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ParsedFlightRow> Merge(
        IReadOnlyList<ParsedFlightRow> rows,
        string? sheetRemark)
    {
        if (rows.Count == 0 || string.IsNullOrWhiteSpace(sheetRemark))
        {
            return rows;
        }

        var footerLines = sheetRemark
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var vipByRow = new Dictionary<int, List<string>>();
        foreach (var line in footerLines)
        {
            var m = VipLineRegex.Match(line.Trim());
            if (!m.Success)
            {
                continue;
            }

            var flightToken = m.Groups[1].Value.Trim().ToUpperInvariant();
            var routeCompact = m.Groups[2].Value.Trim().ToUpperInvariant();
            var timeHhmm = m.Groups[3].Value.Trim().PadLeft(4, '0');
            var paxNote = m.Groups[4].Value.Trim();
            var note = $"{flightToken} {routeCompact} {timeHhmm} — {paxNote}";

            for (var i = 0; i < rows.Count; i++)
            {
                if (MatchesRow(rows[i], flightToken, routeCompact, timeHhmm))
                {
                    if (!vipByRow.TryGetValue(i, out var list))
                    {
                        list = [];
                        vipByRow[i] = list;
                    }

                    list.Add(note);
                }
            }
        }

        if (vipByRow.Count == 0)
        {
            return rows;
        }

        var merged = new List<ParsedFlightRow>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!vipByRow.TryGetValue(i, out var extras))
            {
                merged.Add(row);
                continue;
            }

            var remark = CombineRemarks(row.Remark, extras);
            var isVip = row.IsVip || extras.Count > 0 || Regex.IsMatch(remark ?? "", @"vip", RegexOptions.IgnoreCase);
            merged.Add(new ParsedFlightRow
            {
                ExcelRowNo = row.ExcelRowNo,
                SortOrder = row.SortOrder,
                FlightNo = row.FlightNo,
                DepartureFlightNo = row.DepartureFlightNo,
                Route = row.Route,
                Sta = row.Sta,
                Std = row.Std,
                Eta = row.Eta,
                Etd = row.Etd,
                DepartmentCode = row.DepartmentCode,
                Manning = row.Manning,
                ManningExplain = row.ManningExplain,
                Registration = row.Registration,
                Aircraft = row.Aircraft,
                Carry = row.Carry,
                IsVip = isVip,
                VipNote = isVip ? remark : row.VipNote,
                Gate = row.Gate,
                Belt = row.Belt,
                Parking = row.Parking,
                Remark = remark,
            });
        }

        return merged;
    }

    private static bool MatchesRow(ParsedFlightRow row, string flightToken, string routeCompact, string timeHhmm)
    {
        var arr = row.FlightNo.Trim().ToUpperInvariant();
        var dep = row.DepartureFlightNo?.Trim().ToUpperInvariant();
        if (arr != flightToken && dep != flightToken)
        {
            return false;
        }

        if (!RouteMatches(row.Route, routeCompact))
        {
            return false;
        }

        return TimeMatches(row.Sta, timeHhmm) || TimeMatches(row.Std, timeHhmm);
    }

    private static bool RouteMatches(string route, string compact)
    {
        var key = RouteKey(route);
        return key.Contains(compact, StringComparison.Ordinal)
            || compact.Contains(key, StringComparison.Ordinal);
    }

    private static string RouteKey(string route)
    {
        var parts = route.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sb = new StringBuilder();
        foreach (var p in parts)
        {
            if (p.Length == 3)
            {
                sb.Append(p.ToUpperInvariant());
            }
        }

        return sb.ToString();
    }

    private static bool TimeMatches(string clock, string hhmm)
    {
        var normalized = clock.Replace(":", "", StringComparison.Ordinal)
            .Replace("+", "", StringComparison.Ordinal)
            .Trim();
        if (normalized.Length >= 4)
        {
            normalized = normalized[..4];
        }

        return normalized.PadLeft(4, '0') == hhmm;
    }

    private static string? CombineRemarks(string? inline, IReadOnlyList<string> footerLines)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(inline))
        {
            parts.Add(inline.Trim());
        }

        parts.AddRange(footerLines);
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }
}
