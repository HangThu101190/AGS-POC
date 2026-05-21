using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

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

    public void EnsureMutable(string actionLabel)
    {
        if (IsLocked)
        {
            throw new DomainException(
                "flight_schedule_locked",
                $"Lịch bay tuần {WeekId} đã khóa — không thể {actionLabel}. Chỉ tạo revision / nhập delay.");
        }
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
