using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Planning;

namespace AGS.SmartShift.Domain.Entities.Planning;

/// <summary>TBĐH flight schedule per week — draft → published → locked (HTML tier 0).</summary>
public sealed class FlightSchedule : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public FlightScheduleStatus Status { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? LockedAtUtc { get; private set; }

    private FlightSchedule()
    {
    }

    public static FlightSchedule Create(Guid siteId, string weekId, DateTime utcNow)
    {
        var schedule = new FlightSchedule
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            Status = FlightScheduleStatus.Draft,
        };
        schedule.MarkCreated(utcNow);
        return schedule;
    }

    public bool IsLocked => Status == FlightScheduleStatus.Locked;

    public void EnsureMutable(string actionLabel, int todayIdx)
    {
        if (!IsLocked || !PastDayGuard.IsPastWeek(todayIdx))
        {
            return;
        }

        throw new DomainException(
            "flight_schedule_locked",
            $"Lịch bay tuần {WeekId} đã khóa — không thể {actionLabel}. Chỉ nhập delay trên tuần đã qua.");
    }

    /// <summary>
    /// When locked, flight CRUD/import is blocked only for past ISO weeks.
    /// Current and future weeks stay editable; past days within the current week are gated by <see cref="PastDayGuard"/>.
    /// </summary>
    public void EnsureMutableForDay(int dayIdx, int todayIdx, string actionLabel)
    {
        if (!IsLocked || !PastDayGuard.IsPastWeek(todayIdx))
        {
            return;
        }

        throw new DomainException(
            "flight_schedule_locked",
            $"Lịch bay tuần {WeekId} đã khóa — không thể {actionLabel}. Chỉ nhập delay trên tuần đã qua.");
    }

    public void PublishAndLock(DateTime utcNow)
    {
        Status = FlightScheduleStatus.Locked;
        PublishedAtUtc = utcNow;
        LockedAtUtc = utcNow;
        MarkUpdated(utcNow);
    }

    public void TouchImport(DateTime utcNow)
    {
        if (Status == FlightScheduleStatus.Locked)
        {
            return;
        }

        if (Status == FlightScheduleStatus.Draft)
        {
            MarkUpdated(utcNow);
        }
    }
}
