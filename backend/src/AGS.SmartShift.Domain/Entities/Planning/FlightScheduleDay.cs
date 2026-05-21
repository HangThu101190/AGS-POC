using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

/// <summary>Per-day flight plan metadata from Excel import (footer Remark block, source label).</summary>
public sealed class FlightScheduleDay : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public string? SourceDayLabel { get; private set; }
    public string? SheetRemark { get; private set; }

    private FlightScheduleDay()
    {
    }

    public static FlightScheduleDay Create(
        Guid siteId,
        string weekId,
        int dayIdx,
        DateTime utcNow,
        string? sourceDayLabel = null,
        string? sheetRemark = null)
    {
        var row = new FlightScheduleDay
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            SourceDayLabel = sourceDayLabel?.Trim(),
            SheetRemark = string.IsNullOrWhiteSpace(sheetRemark) ? null : sheetRemark.Trim(),
        };
        row.MarkCreated(utcNow);
        return row;
    }

    public void UpdateFromImport(string? sourceDayLabel, string? sheetRemark, DateTime utcNow)
    {
        SourceDayLabel = sourceDayLabel?.Trim();
        SheetRemark = string.IsNullOrWhiteSpace(sheetRemark) ? null : sheetRemark.Trim();
        MarkUpdated(utcNow);
    }
}
