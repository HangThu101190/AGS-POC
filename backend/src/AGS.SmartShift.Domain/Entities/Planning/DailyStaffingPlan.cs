using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class DailyStaffingPlan : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public string DepartmentCode { get; private set; } = string.Empty;
    public DailyStaffingPlanStatus Status { get; private set; }
    public string? HeaderJson { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }

    private DailyStaffingPlan()
    {
    }

    public static DailyStaffingPlan Create(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        DateTime utcNow,
        string? headerJson = null)
    {
        var plan = new DailyStaffingPlan
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            DepartmentCode = departmentCode.Trim().ToUpperInvariant(),
            Status = DailyStaffingPlanStatus.TbdhDraft,
            HeaderJson = headerJson,
        };
        plan.MarkCreated(utcNow);
        return plan;
    }

    public void SetStatus(DailyStaffingPlanStatus status, DateTime utcNow)
    {
        Status = status;
        if (status == DailyStaffingPlanStatus.Confirmed)
        {
            ConfirmedAtUtc = utcNow;
        }

        MarkUpdated(utcNow);
    }

    public void UpdateHeaderJson(string? headerJson, DateTime utcNow)
    {
        HeaderJson = headerJson;
        MarkUpdated(utcNow);
    }
}
