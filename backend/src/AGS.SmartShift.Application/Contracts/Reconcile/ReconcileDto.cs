namespace AGS.SmartShift.Application.Contracts.Reconcile;

public sealed class ReconcileSummaryDto
{
    public string WeekId { get; init; } = string.Empty;

    public string DepartmentCode { get; init; } = string.Empty;

    public IReadOnlyList<ReconcileEmployeeDto> Employees { get; init; } = [];
}

public sealed class ReconcileEmployeeDto
{
    public Guid EmployeeId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<ReconcileDayDto> Days { get; init; } = [];

    public bool HasFlag { get; init; }
}

public sealed class ReconcileDayDto
{
    public int DayIdx { get; init; }

    public string DateLabel { get; init; } = string.Empty;

    public string PlannedCode { get; init; } = string.Empty;

    public string ActualCode { get; init; } = string.Empty;

    public string CodeType { get; init; } = "work";

    public bool Mismatch { get; init; }

    public bool HasRevision { get; init; }

    public bool HasCheckIn { get; init; }

    public double PlannedTotalHours { get; init; }

    public double ActualTotalHours { get; init; }

    public string[] PlannedSegments { get; init; } = [];

    public string[] ActualSegments { get; init; } = [];

    public string? RevisionReason { get; init; }

    public string? RevisionBy { get; init; }
}
