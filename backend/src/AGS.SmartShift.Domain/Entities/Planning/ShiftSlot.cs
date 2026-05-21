using System.Text.Json;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class ShiftSlot : Entity<Guid>
{
    public Guid WeeklyPlanId { get; private set; }
    public string DepartmentCode { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public string SegmentsJson { get; private set; } = "[]";
    public int Headcount { get; private set; }
    public string? BucketKey { get; private set; }
    public string FlightNosJson { get; private set; } = "[]";

    private ShiftSlot()
    {
    }

    public static ShiftSlot Create(
        Guid weeklyPlanId,
        string departmentCode,
        int dayIdx,
        IReadOnlyList<string> segments,
        int headcount,
        IReadOnlyList<string> flightNos,
        string? bucketKey = null)
    {
        return new ShiftSlot
        {
            Id = Guid.NewGuid(),
            WeeklyPlanId = weeklyPlanId,
            DepartmentCode = departmentCode.Trim().ToUpperInvariant(),
            DayIdx = dayIdx,
            SegmentsJson = JsonSerializer.Serialize(segments),
            Headcount = Math.Max(1, headcount),
            BucketKey = bucketKey,
            FlightNosJson = JsonSerializer.Serialize(flightNos),
        };
    }

    public IReadOnlyList<string> GetSegments() =>
        JsonSerializer.Deserialize<List<string>>(SegmentsJson) ?? [];

    public IReadOnlyList<string> GetFlightNos() =>
        JsonSerializer.Deserialize<List<string>>(FlightNosJson) ?? [];

    public void ApplySegments(IReadOnlyList<string> segments)
    {
        SegmentsJson = JsonSerializer.Serialize(segments);
    }
}
