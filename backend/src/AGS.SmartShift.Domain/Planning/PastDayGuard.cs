namespace AGS.SmartShift.Domain.Planning;

public static class PastDayGuard
{
    /// <summary>ISO week is before the operational current week (<c>TodayIdx</c> is 7).</summary>
    public static bool IsPastWeek(int todayIdx) => todayIdx > 6;

    public static bool IsPastDay(int dayIdx, int todayIdx) => dayIdx < todayIdx;

    public static void EnsureMutableDay(int dayIdx, int todayIdx, string action)
    {
        if (!IsPastDay(dayIdx, todayIdx))
        {
            return;
        }

        throw new PastDayMutationException(action);
    }
}

public sealed class PastDayMutationException : Common.DomainException
{
    public PastDayMutationException(string action)
        : base("past_day_read_only", $"Ngày đã qua — không thể {action}.")
    {
    }
}
