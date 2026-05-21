namespace AGS.SmartShift.Domain.Planning;

/// <summary>
/// STA→STD (or ETA→ETD when delayed) must intersect at least one shift segment (prototype parity).
/// </summary>
public static class FlightSlotOverlap
{
    public static bool OverlapsSlotSegments(
        string sta,
        string std,
        bool isDelayed,
        string? eta,
        string? etd,
        IReadOnlyList<string> segments)
    {
        if (segments.Count == 0)
        {
            return true;
        }

        var flightInterval = FlightServiceIntervalOpen(sta, std, isDelayed, eta, etd);
        if (flightInterval is null)
        {
            return true;
        }

        var (fs, fe) = flightInterval.Value;
        return segments.Any(seg =>
        {
            var span = SegmentSpanDayMinutes(seg);
            return span is not null && IntervalsOverlapOpen(fs, fe, span.Value.start, span.Value.end);
        });
    }

    internal static (int start, int end)? FlightServiceIntervalOpen(
        string sta,
        string std,
        bool isDelayed,
        string? eta,
        string? etd)
    {
        var staLbl = isDelayed && !string.IsNullOrWhiteSpace(eta) ? eta! : sta;
        var stdLbl = isDelayed && !string.IsNullOrWhiteSpace(etd) ? etd! : std;
        var a = ParseHhMmToDayMinutes(staLbl);
        if (a is null)
        {
            return null;
        }

        var start = a.Value.mins;
        var b = ParseHhMmToDayMinutes(stdLbl);
        int end;
        if (b is null)
        {
            end = start + 60;
        }
        else
        {
            var endM = b.Value.mins;
            if (b.Value.hasPlus || endM <= start)
            {
                endM += 24 * 60;
            }

            end = endM;
        }

        if (end <= start)
        {
            end = start + 30;
        }

        return (start, end);
    }

    internal static (int start, int end)? SegmentSpanDayMinutes(string segStr)
    {
        var parts = segStr.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !int.TryParse(parts[0], out var sa)
            || !int.TryParse(parts[1], out var sb))
        {
            return null;
        }

        var start = sa * 60;
        var end = sb * 60;
        if (sb <= sa)
        {
            end += 24 * 60;
        }

        return (start, end);
    }

    internal static bool IntervalsOverlapOpen(int a0, int a1, int b0, int b1) => a0 < b1 && a1 > b0;

    private static (int mins, bool hasPlus)? ParseHhMmToDayMinutes(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var s = label.Trim();
        var hasPlus = s.Contains('+', StringComparison.Ordinal);
        var core = s.Split('+')[0].Trim();
        var parts = core.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 2
            || !int.TryParse(parts[0], out var h)
            || !int.TryParse(parts[1], out var m))
        {
            return null;
        }

        return (h * 60 + m, hasPlus);
    }
}
