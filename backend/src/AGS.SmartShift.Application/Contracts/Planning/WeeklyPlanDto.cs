using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class WeeklyPlanDto
{
    public Guid Id { get; init; }
    public string WeekId { get; init; } = string.Empty;
    public int WeekYear { get; init; }
    public int TodayIdx { get; init; }
    public IReadOnlyList<string> WeekDates { get; init; } = [];
    public PlanStatus Status { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public IReadOnlyList<ShiftSlotDto> Slots { get; init; } = [];
}

public sealed class ShiftSlotDto
{
    public Guid Id { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public IReadOnlyList<string> Segments { get; init; } = [];
    public int Headcount { get; init; }
    public string? BucketKey { get; init; }
    public IReadOnlyList<string> FlightNos { get; init; } = [];
}
