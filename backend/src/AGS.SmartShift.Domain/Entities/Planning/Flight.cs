using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class Flight : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    /// <summary>Excel column NO — stable row order within the day.</summary>
    public int ExcelRowNo { get; private set; }
    public int SortOrder { get; private set; }
    public string FlightNo { get; private set; } = string.Empty;
    public string? DepartureFlightNo { get; private set; }
    public string Route { get; private set; } = string.Empty;
    public string Sta { get; private set; } = string.Empty;
    public string Std { get; private set; } = string.Empty;
    public string? Eta { get; private set; }
    public string? Etd { get; private set; }
    public int EtaDelayMinutes { get; private set; }
    public int EtdDelayMinutes { get; private set; }
    /// <summary>Legacy max(ETA, ETD) delay — used for sync proposals and notifications.</summary>
    public int DelayMinutes { get; private set; }
    public bool IsDelayed { get; private set; }
    public string? Registration { get; private set; }
    public string? Aircraft { get; private set; }
    public string? Carry { get; private set; }
    public string DepartmentCode { get; private set; } = string.Empty;
    public int Manning { get; private set; }
    public string? ManningExplain { get; private set; }
    public bool IsVip { get; private set; }
    public string? VipNote { get; private set; }
    public string? Gate { get; private set; }
    public string? Belt { get; private set; }
    public string? Parking { get; private set; }
    public string? Remark { get; private set; }

    private Flight()
    {
    }

    public static Flight Create(
        Guid siteId,
        string weekId,
        int dayIdx,
        string flightNo,
        string? departureFlightNo,
        string route,
        string sta,
        string std,
        string departmentCode,
        int manning,
        DateTime utcNow,
        string? aircraft = null,
        string? manningExplain = null,
        bool isVip = false,
        string? vipNote = null,
        string? gate = null,
        string? belt = null,
        string? parking = null,
        string? remark = null,
        string? eta = null,
        string? etd = null,
        string? registration = null,
        string? carry = null,
        int excelRowNo = 0,
        int sortOrder = 0)
    {
        var flight = new Flight
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            ExcelRowNo = excelRowNo > 0 ? excelRowNo : sortOrder,
            SortOrder = sortOrder > 0 ? sortOrder : excelRowNo,
            FlightNo = flightNo.Trim().ToUpperInvariant(),
            DepartureFlightNo = string.IsNullOrWhiteSpace(departureFlightNo)
                ? null
                : departureFlightNo.Trim().ToUpperInvariant(),
            Route = route.Trim(),
            Sta = sta.Trim(),
            Std = std.Trim(),
            DepartmentCode = departmentCode.Trim().ToUpperInvariant(),
            Manning = Math.Max(1, manning),
            Eta = string.IsNullOrWhiteSpace(eta) ? null : eta.Trim(),
            Etd = string.IsNullOrWhiteSpace(etd) ? null : etd.Trim(),
            Registration = string.IsNullOrWhiteSpace(registration) ? null : registration.Trim().ToUpperInvariant(),
            Aircraft = aircraft?.Trim(),
            Carry = string.IsNullOrWhiteSpace(carry) ? null : carry.Trim().ToUpperInvariant(),
            ManningExplain = manningExplain?.Trim(),
            IsVip = isVip,
            VipNote = vipNote?.Trim(),
            Gate = gate?.Trim(),
            Belt = belt?.Trim(),
            Parking = parking?.Trim(),
            Remark = remark?.Trim(),
        };
        flight.MarkCreated(utcNow);
        return flight;
    }

    public void UpdateDetails(
        string route,
        string sta,
        string std,
        string departmentCode,
        int manning,
        DateTime utcNow,
        string? departureFlightNo = null,
        string? aircraft = null,
        string? manningExplain = null,
        bool? isVip = null,
        string? vipNote = null,
        string? gate = null,
        string? belt = null,
        string? parking = null,
        string? remark = null)
    {
        Route = route.Trim();
        Sta = sta.Trim();
        Std = std.Trim();
        DepartmentCode = departmentCode.Trim().ToUpperInvariant();
        Manning = Math.Max(1, manning);
        DepartureFlightNo = string.IsNullOrWhiteSpace(departureFlightNo)
            ? null
            : departureFlightNo.Trim().ToUpperInvariant();
        Aircraft = aircraft?.Trim();
        ManningExplain = manningExplain?.Trim();
        if (isVip is bool vip)
        {
            IsVip = vip;
        }

        VipNote = vipNote?.Trim();
        Gate = gate?.Trim();
        Belt = belt?.Trim();
        Parking = parking?.Trim();
        Remark = remark?.Trim();
        MarkUpdated(utcNow);
    }

    public void ApplyDelay(int delayMinutes, DateTime utcNow) =>
        ApplyDelays(delayMinutes, delayMinutes, utcNow);

    public void ApplyDelays(int etaDelayMinutes, int etdDelayMinutes, DateTime utcNow)
    {
        EtaDelayMinutes = Math.Max(0, etaDelayMinutes);
        EtdDelayMinutes = Math.Max(0, etdDelayMinutes);
        DelayMinutes = Math.Max(EtaDelayMinutes, EtdDelayMinutes);
        IsDelayed = DelayMinutes > 0;
        Eta = EtaDelayMinutes > 0 ? AddMinutesToClock(Sta, EtaDelayMinutes) : null;
        Etd = EtdDelayMinutes > 0 ? AddMinutesToClock(Std, EtdDelayMinutes) : null;
        MarkUpdated(utcNow);
    }

    private static string AddMinutesToClock(string hhmm, int minutes)
    {
        var parts = hhmm.Replace("+", "").Split(':');
        if (parts.Length < 2 || !int.TryParse(parts[0], out var h) || !int.TryParse(parts[1], out var m))
        {
            return hhmm;
        }

        var total = h * 60 + m + minutes;
        var nh = (total / 60) % 24;
        var nm = total % 60;
        var suffix = hhmm.Contains('+', StringComparison.Ordinal) || total >= 24 * 60 ? "+" : "";
        return $"{nh:D2}:{nm:D2}{suffix}";
    }
}
