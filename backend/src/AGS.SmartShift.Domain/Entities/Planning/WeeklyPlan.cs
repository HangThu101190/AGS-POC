using System.Text.Json;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class WeeklyPlan : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int WeekYear { get; private set; }
    public int TodayIdx { get; private set; }
    public string WeekDatesJson { get; private set; } = "[]";
    public PlanStatus Status { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public List<ShiftSlot> Slots { get; private set; } = [];

    private WeeklyPlan()
    {
    }

    public static WeeklyPlan Create(
        Guid siteId,
        string weekId,
        int weekYear,
        int todayIdx,
        IReadOnlyList<string> weekDates,
        DateTime utcNow)
    {
        var plan = new WeeklyPlan
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            WeekId = weekId.Trim(),
            WeekYear = weekYear,
            TodayIdx = todayIdx,
            WeekDatesJson = JsonSerializer.Serialize(weekDates),
            Status = PlanStatus.Draft,
        };
        plan.MarkCreated(utcNow);
        return plan;
    }

    public IReadOnlyList<string> GetWeekDates() =>
        JsonSerializer.Deserialize<List<string>>(WeekDatesJson) ?? [];

    public void ReplaceSlotsForDays(
        IEnumerable<ShiftSlot> newSlots,
        int todayIdx,
        DateTime utcNow)
    {
        var incoming = newSlots.ToList();
        Slots.RemoveAll(s => s.DayIdx >= todayIdx);
        Slots.AddRange(incoming);
        MarkUpdated(utcNow);
    }

    public void ResetFuture(int todayIdx, DateTime utcNow)
    {
        Slots.RemoveAll(s => s.DayIdx >= todayIdx);
        Status = PlanStatus.Draft;
        PublishedAtUtc = null;
        MarkUpdated(utcNow);
    }

    public void Publish(DateTime utcNow)
    {
        Status = PlanStatus.Published;
        PublishedAtUtc = utcNow;
        MarkUpdated(utcNow);
    }

    public void MarkInProgress(DateTime utcNow)
    {
        if (Status == PlanStatus.Published)
        {
            Status = PlanStatus.InProgress;
            MarkUpdated(utcNow);
        }
    }

    public void Touch(DateTime utcNow) => MarkUpdated(utcNow);
}
