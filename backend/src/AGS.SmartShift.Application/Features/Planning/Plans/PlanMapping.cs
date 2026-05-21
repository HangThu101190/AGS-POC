using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

internal static class PlanMapping
{
    public static WeeklyPlanDto ToDto(WeeklyPlan plan, DateTime? referenceLocalDate = null)
    {
        var refDate = (referenceLocalDate ?? DateTime.UtcNow.AddHours(7)).Date;
        var todayIdx = plan.TodayIdx;
        var weekDates = plan.GetWeekDates();
        if (WeekCalendar.TryParseWeekId(plan.WeekId, out var isoYear, out var isoWeek))
        {
            (_, _, todayIdx, weekDates) = WeekCalendar.BuildWeek(isoYear, isoWeek, refDate);
        }

        return new()
        {
        Id = plan.Id,
        WeekId = plan.WeekId,
        WeekYear = plan.WeekYear,
        TodayIdx = todayIdx,
        WeekDates = weekDates.ToList(),
        Status = plan.Status,
        PublishedAtUtc = plan.PublishedAtUtc,
        Slots = plan.Slots.Select(ToSlotDto).ToList(),
        };
    }

    private static ShiftSlotDto ToSlotDto(ShiftSlot slot) => new()
    {
        Id = slot.Id,
        DepartmentCode = slot.DepartmentCode,
        DayIdx = slot.DayIdx,
        Segments = slot.GetSegments(),
        Headcount = slot.Headcount,
        BucketKey = slot.BucketKey,
        FlightNos = slot.GetFlightNos(),
    };
}
