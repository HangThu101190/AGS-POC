namespace AGS.SmartShift.Application.Contracts.Attendance;

public sealed class StaffTodayShiftDto
{
    public string PlanStatus { get; init; } = "draft";

    public IReadOnlyList<string> Segments { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> FlightNos { get; init; } = Array.Empty<string>();

    /// <summary>Unread <c>shift_changed</c> notification — show “Ca đã thay đổi” banner (prototype TodayTab).</summary>
    public bool ShiftChanged { get; init; }
}
