using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Attendance;

public sealed class ShiftCheckInPolicy : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string? DepartmentCode { get; private set; }
    public string BucketKey { get; private set; } = string.Empty;
    public string SegmentStart { get; private set; } = "06:00";
    public string SegmentEnd { get; private set; } = "14:00";
    public int CheckInEarliestMinutesBefore { get; private set; }
    public int CheckInLatestMinutesAfterStart { get; private set; }
    public int CheckOutEarliestMinutesBeforeEnd { get; private set; }
    public int CheckOutLatestMinutesAfterEnd { get; private set; }
    public bool RequireNoteWhenLate { get; private set; }
    public bool IsActive { get; private set; }

    private ShiftCheckInPolicy()
    {
    }

    public static ShiftCheckInPolicy CreateDefault(
        Guid siteId,
        string bucketKey,
        string segmentStart,
        string segmentEnd,
        DateTime utcNow,
        string? departmentCode = null)
    {
        var policy = new ShiftCheckInPolicy
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            DepartmentCode = departmentCode?.Trim().ToUpperInvariant(),
            BucketKey = bucketKey,
            SegmentStart = segmentStart,
            SegmentEnd = segmentEnd,
            CheckInEarliestMinutesBefore = 30,
            CheckInLatestMinutesAfterStart = 15,
            CheckOutEarliestMinutesBeforeEnd = 0,
            CheckOutLatestMinutesAfterEnd = 30,
            RequireNoteWhenLate = true,
            IsActive = true,
        };
        policy.MarkCreated(utcNow);
        return policy;
    }
}
