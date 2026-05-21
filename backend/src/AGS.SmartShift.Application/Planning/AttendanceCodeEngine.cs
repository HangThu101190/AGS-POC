namespace AGS.SmartShift.Application.Planning;

/// <summary>Port of prototype <c>codeFromSegments</c> — configurable via <see cref="AttendanceCodeFlags"/>.</summary>
public static class AttendanceCodeEngine
{
    private static readonly HashSet<string> Holidays = new(StringComparer.Ordinal)
    {
        "01/01", "30/04", "01/05", "02/09",
    };

    public static AttendanceCodeResult FromSegments(
        IReadOnlyList<string> segments,
        string? dateLabel = null,
        AttendanceCodeFlags? flags = null)
    {
        flags ??= AttendanceCodeFlags.None;

        if (!string.IsNullOrWhiteSpace(flags.LeaveCode))
        {
            return new AttendanceCodeResult(flags.LeaveCode, "leave", 0, 0, 0);
        }

        if (segments.Count == 0)
        {
            return new AttendanceCodeResult("T", "weekoff", 0, 0, 0);
        }

        var (dayH, nightH) = SumDayNightHours(segments);
        var total = dayH + nightH;

        if (!string.IsNullOrWhiteSpace(dateLabel) && Holidays.Contains(NormalizeDateLabel(dateLabel)))
        {
            return new AttendanceCodeResult("L", "holiday", total, dayH, nightH);
        }

        var parts = new List<string>();
        if (dayH > 0)
        {
            parts.Add($"N{dayH}");
        }

        if (nightH > 0)
        {
            parts.Add($"D{nightH}");
        }

        var code = parts.Count > 0 ? string.Join(" + ", parts) : "—";
        return new AttendanceCodeResult(code, "work", total, dayH, nightH);
    }

    private static string NormalizeDateLabel(string dateLabel)
    {
        var t = dateLabel.Trim();
        return t.Length >= 5 ? t[..5] : t;
    }

    private static (int DayH, int NightH) SumDayNightHours(IReadOnlyList<string> segments)
    {
        var dayH = 0;
        var nightH = 0;
        foreach (var seg in segments)
        {
            if (!TryParseSegmentHours(seg, out var startHour, out var endHour))
            {
                continue;
            }

            var e = endHour;
            if (e <= startHour)
            {
                e += 24;
            }

            for (var h = startHour; h < e; h++)
            {
                var hour = h % 24;
                if (hour >= 22 || hour < 6)
                {
                    nightH++;
                }
                else
                {
                    dayH++;
                }
            }
        }

        return (dayH, nightH);
    }

    private static bool TryParseSegmentHours(string segment, out int startHour, out int endHour)
    {
        startHour = 0;
        endHour = 0;
        var parts = segment.Split('-', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (TryParseHourToken(parts[0], out startHour) && TryParseHourToken(parts[1], out endHour))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseHourToken(string token, out int hour)
    {
        hour = 0;
        var t = token.Replace("+", "").Trim();
        if (t.Contains(':', StringComparison.Ordinal))
        {
            var bits = t.Split(':');
            return bits.Length >= 1 && int.TryParse(bits[0], out hour);
        }

        return int.TryParse(t, out hour);
    }
}

public sealed class AttendanceCodeFlags
{
    public static AttendanceCodeFlags None { get; } = new();

    public string? LeaveCode { get; init; }
}

public sealed record AttendanceCodeResult(
    string Code,
    string Type,
    double TotalHours,
    double DayHours,
    double NightHours);
