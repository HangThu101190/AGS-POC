using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Contracts.Staffing;

public sealed class StaffingDayDto
{
    public required StaffingPlanDto Plan { get; init; }
    public required IReadOnlyList<StaffingLineDto> Lines { get; init; }
    public required IReadOnlyList<StaffingFlightDto> Flights { get; init; }
    public required IReadOnlyList<StaffingAssignmentDto> Assignments { get; init; }
}

public sealed class StaffingPlanDto
{
    public Guid Id { get; init; }
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public DailyStaffingPlanStatus Status { get; init; }
    public string? HeaderJson { get; init; }
    public DateTime? ConfirmedAt { get; init; }
}

public sealed class StaffingLineDto
{
    public Guid Id { get; init; }
    public Guid FlightId { get; init; }
    public OperationalSegment Segment { get; init; }
    public int SortOrder { get; init; }
    public int TargetManning { get; init; }
    public int ProposedManning { get; init; }
}

public sealed class StaffingFlightDto
{
    public Guid Id { get; init; }
    public string FlightNo { get; init; } = string.Empty;
    public string? DepartureFlightNo { get; init; }
    public string Route { get; init; } = string.Empty;
    public string Sta { get; init; } = string.Empty;
    public string Std { get; init; } = string.Empty;
    public string? Eta { get; init; }
    public string? Etd { get; init; }
    public int EtaDelayMinutes { get; init; }
    public int EtdDelayMinutes { get; init; }
    public int DelayMinutes { get; init; }
    public bool IsDelayed { get; init; }
    public string? Aircraft { get; init; }
    public bool IsVip { get; init; }
    public int Manning { get; init; }
    public string? Belt { get; init; }
}

public sealed class StaffingAssignmentDto
{
    public Guid Id { get; init; }
    public Guid StaffingLineId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    /// <summary>Phòng ban của nhân viên (mã), dùng hiển thị thay cho vai trò General trên UI.</summary>
    public string EmployeeDepartmentCode { get; init; } = string.Empty;
    public CrewRole Role { get; init; }
    public string WorkStart { get; init; } = string.Empty;
    public string WorkEnd { get; init; } = string.Empty;
    public bool IsOvertime { get; init; }
    public AttendanceComplianceDto? Attendance { get; init; }
}

public sealed class AttendanceComplianceDto
{
    public string Status { get; init; } = "not_checked_in";
    public DateTime? CheckInAt { get; init; }
    public DateTime? CheckOutAt { get; init; }
    public string? GeoNote { get; init; }
    public string? LateCheckInNote { get; init; }
    public string? ValidFrom { get; init; }
    public string? ValidUntil { get; init; }
}

public sealed class StaffingRosterEntryDto
{
    public Guid EmployeeId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DepartmentCode { get; init; } = string.Empty;
    public bool Eligible { get; init; }
    public bool EligibleAsOvertime { get; init; }
    public DayAvailabilityStatus? AvailabilityStatus { get; init; }
    public string? LeaveTypeCode { get; init; }
    public IReadOnlyList<Guid> AssignedLineIds { get; init; } = [];
}

public sealed class PatchStaffingLineDto
{
    public int TargetManning { get; init; }
}

public sealed class CreateStaffingAssignmentDto
{
    public Guid StaffingLineId { get; init; }
    public Guid EmployeeId { get; init; }
    public CrewRole Role { get; init; }
    public string WorkStart { get; init; } = "06:00";
    public string WorkEnd { get; init; } = "14:00";
    public bool IsOvertime { get; init; }
}

public sealed class UpdateStaffingAssignmentDto
{
    public CrewRole Role { get; init; }
    public string WorkStart { get; init; } = "06:00";
    public string WorkEnd { get; init; } = "14:00";
    public bool IsOvertime { get; init; }
}

public sealed class ConfirmStaffingResultDto
{
    public int SlotsUpdated { get; init; }
    public int AssignmentsCreated { get; init; }
    public StaffingPlanDto Plan { get; init; } = null!;
}

public sealed class PvhkExportRequest
{
    public required StaffingDayDto Day { get; init; }
    public required string DayLabel { get; init; }
    public int CalendarYear { get; init; } = DateTime.UtcNow.Year;
}

public sealed class PvhkWeekExportRequest
{
    public required IReadOnlyList<PvhkExportRequest> Days { get; init; }
}
