using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Contracts.Staffing;

public sealed class ShiftTemplateDto
{
    public Guid Id { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public bool IsOvernight { get; init; }
    public decimal MaxHours { get; init; }
    public OperationalSegment? Segment { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public sealed class UpsertShiftTemplateDto
{
    public string DepartmentCode { get; init; } = "PVHK_DI";
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string StartTime { get; init; } = "06:00";
    public string EndTime { get; init; } = "16:00";
    public bool IsOvernight { get; init; }
    public decimal MaxHours { get; init; } = 8m;
    public OperationalSegment? Segment { get; init; }
    public int SortOrder { get; init; }
}

public sealed class EmployeeQualificationDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public OperationalSegment Segment { get; init; }
    public CrewRole CrewRole { get; init; }
    public int Proficiency { get; init; }
    public bool IsActive { get; init; }
}

public sealed class EmployeeQualificationEntryDto
{
    public OperationalSegment Segment { get; init; }
    public CrewRole CrewRole { get; init; }
    public int Proficiency { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpsertEmployeeQualificationsDto
{
    public IReadOnlyList<EmployeeQualificationEntryDto> Qualifications { get; init; } = [];
}

public sealed class StaffingBioHeaderDto
{
    public Guid? MorningSupEmployeeId { get; init; }
    public string? MorningSupName { get; init; }
    public Guid? EveningSupEmployeeId { get; init; }
    public string? EveningSupName { get; init; }
    public string? RadioNote { get; init; }
    public Guid? AssignerEmployeeId { get; init; }
    public string? AssignerName { get; init; }
}

public sealed class UpsertStaffingBioHeaderDto
{
    public Guid? MorningSupEmployeeId { get; init; }
    public Guid? EveningSupEmployeeId { get; init; }
    public string? RadioNote { get; init; }
    public Guid? AssignerEmployeeId { get; init; }
}

public sealed class AutoAssignStaffingResultDto
{
    public IReadOnlyList<StaffingCrewProposalDto> Proposals { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public AutoAssignSummaryDto Summary { get; init; } = new();
}

public sealed class StaffingCrewProposalDto
{
    public Guid Id { get; init; }
    public Guid StaffingLineId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public Guid? ShiftTemplateId { get; init; }
    public string? ShiftTemplateCode { get; init; }
    public CrewRole CrewRole { get; init; }
    public string WorkStart { get; init; } = string.Empty;
    public string WorkEnd { get; init; } = string.Empty;
    public bool IsOvertime { get; init; }
    public bool IsAutoAssigned { get; init; }
    public string? Note { get; init; }
}

public sealed class AutoAssignSummaryDto
{
    public int TotalEmployees { get; init; }
    public int TotalOvertime { get; init; }
    public decimal CoveragePercent { get; init; }
    public int UnfilledSlots { get; init; }
}

public sealed class PublishStaffingDayDto
{
    public string? LockReason { get; init; }
}

public sealed class PublishStaffingResultDto
{
    public DailyStaffingPlanStatus Status { get; init; }
    public DateTime PublishedAt { get; init; }
    public int NotificationsSent { get; init; }
}
