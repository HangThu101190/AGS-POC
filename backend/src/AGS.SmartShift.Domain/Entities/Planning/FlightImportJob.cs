using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class FlightImportJob : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public FlightImportJobStatus Status { get; private set; }
    public int ProgressPercent { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ResultJson { get; private set; }

    private FlightImportJob()
    {
    }

    public static FlightImportJob Create(
        Guid siteId,
        string weekId,
        int dayIdx,
        Guid createdByUserId,
        DateTime utcNow)
    {
        var job = new FlightImportJob
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            CreatedByUserId = createdByUserId,
            Status = FlightImportJobStatus.Pending,
            ProgressPercent = 0,
        };
        job.MarkCreated(utcNow);
        return job;
    }

    public void MarkRunning(int progress, DateTime utcNow)
    {
        Status = FlightImportJobStatus.Running;
        ProgressPercent = Math.Clamp(progress, 0, 100);
        MarkUpdated(utcNow);
    }

    public void MarkCompleted(string? resultJson, DateTime utcNow)
    {
        Status = FlightImportJobStatus.Completed;
        ProgressPercent = 100;
        ResultJson = resultJson;
        ErrorMessage = null;
        MarkUpdated(utcNow);
    }

    public void MarkFailed(string error, DateTime utcNow)
    {
        Status = FlightImportJobStatus.Failed;
        ErrorMessage = error;
        MarkUpdated(utcNow);
    }
}
