using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Planning;

public static class ShiftSlotGenerator
{
    private static readonly ShiftBucket[] Buckets =
    [
        new("MORNING", ["06-14"], 6, 14),
        new("AFTERNOON", ["14-22"], 14, 22),
        new("NIGHT", ["22-06"], 22, 30),
    ];

    private static readonly Dictionary<string, double> DeptHeadWeight = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PVHK_DI"] = 0.6,
        ["PVHK_DEN"] = 0.45,
        ["RAMP"] = 0.45,
        ["BAGGAGE"] = 0.35,
    };

    public static IReadOnlyList<ShiftSlot> GenerateForWeek(
        Guid weeklyPlanId,
        IReadOnlyList<Flight> flights,
        IReadOnlyList<string> departmentCodes,
        int todayIdx,
        IReadOnlyDictionary<string, int> staffCountByDept)
    {
        var slots = new List<ShiftSlot>();
        var maxDay = flights.Count == 0 ? 6 : flights.Max(f => f.DayIdx);

        for (var dayIdx = todayIdx; dayIdx <= maxDay; dayIdx++)
        {
            var dayFlights = flights.Where(f => f.DayIdx == dayIdx).ToList();
            if (dayFlights.Count == 0)
            {
                continue;
            }

            foreach (var dept in departmentCodes)
            {
                slots.AddRange(BucketFlightsToSlots(weeklyPlanId, dayFlights, dept, dayIdx, staffCountByDept));
            }
        }

        return slots;
    }

    private static IEnumerable<ShiftSlot> BucketFlightsToSlots(
        Guid weeklyPlanId,
        IReadOnlyList<Flight> flights,
        string dept,
        int dayIdx,
        IReadOnlyDictionary<string, int> staffCountByDept)
    {
        var deptStaff = staffCountByDept.TryGetValue(dept, out var count) ? count : 0;

        foreach (var bucket in Buckets)
        {
            var inBucket = flights.Where(f =>
                string.Equals(f.DepartmentCode, dept, StringComparison.OrdinalIgnoreCase)
                && StaInBucket(f.Sta, bucket)).ToList();
            if (inBucket.Count == 0)
            {
                continue;
            }

            var weight = DeptHeadWeight.GetValueOrDefault(dept, 0.4);
            var raw = Math.Max(1, (int)Math.Ceiling(inBucket.Count * weight));
            var headcount = deptStaff > 0 ? Math.Min(raw, deptStaff) : raw;
            yield return ShiftSlot.Create(
                weeklyPlanId,
                dept,
                dayIdx,
                bucket.Segments,
                headcount,
                inBucket.Select(f => f.FlightNo).ToList(),
                bucket.Key);
        }
    }

    private static bool StaInBucket(string sta, ShiftBucket bucket)
    {
        var staText = sta ?? "";
        var hhText = staText.Split(':')[0];
        if (!int.TryParse(hhText, out var hh))
        {
            return false;
        }

        if (bucket.Key == "NIGHT")
        {
            return hh >= 22 || staText.Contains('+', StringComparison.Ordinal) || hh < 6;
        }

        return hh >= bucket.StaMin && hh < bucket.StaMax;
    }

    private sealed record ShiftBucket(string Key, string[] Segments, int StaMin, int StaMax);
}
