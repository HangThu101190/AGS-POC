using System.Globalization;

namespace AGS.SmartShift.Domain.Planning;

public static class WeekCalendar
{
    public static string FormatWeekId(DateTime date)
    {
        var year = ISOWeek.GetYear(date);
        var week = ISOWeek.GetWeekOfYear(date);
        return $"{year}-W{week:D2}";
    }

    public static bool TryParseWeekId(string weekId, out int isoYear, out int isoWeek)
    {
        isoYear = 0;
        isoWeek = 0;
        if (string.IsNullOrWhiteSpace(weekId))
        {
            return false;
        }

        var parts = weekId.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[1].StartsWith("W", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out isoYear))
        {
            return false;
        }

        if (!int.TryParse(parts[1][1..], NumberStyles.None, CultureInfo.InvariantCulture, out isoWeek))
        {
            return false;
        }

        return isoWeek is >= 1 and <= 53;
    }

    public static (string WeekId, int WeekYear, int TodayIdx, IReadOnlyList<string> WeekDates) BuildCurrentWeek(
        DateTime localDate) =>
        BuildWeek(ISOWeek.GetYear(localDate), ISOWeek.GetWeekOfYear(localDate), localDate);

    public static (string WeekId, int WeekYear, int TodayIdx, IReadOnlyList<string> WeekDates) BuildWeek(
        int isoYear,
        int isoWeek,
        DateTime referenceLocalDate)
    {
        var weekId = $"{isoYear}-W{isoWeek:D2}";
        var monday = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
        var sunday = monday.AddDays(6);
        var dates = Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i).ToString("dd/MM", CultureInfo.InvariantCulture))
            .ToList();

        var refDate = referenceLocalDate.Date;
        int todayIdx;
        if (refDate < monday)
        {
            todayIdx = -1;
        }
        else if (refDate > sunday)
        {
            todayIdx = 7;
        }
        else
        {
            todayIdx = refDate.DayOfWeek switch
            {
                DayOfWeek.Sunday => 6,
                _ => (int)refDate.DayOfWeek - 1,
            };
        }

        return (weekId, isoYear, todayIdx, dates);
    }
}
