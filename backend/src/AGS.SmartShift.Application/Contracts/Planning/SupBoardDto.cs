namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class SupBoardDto
{
    public string WeekId { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public string PlanStatus { get; init; } = "draft";
    public IReadOnlyList<SupBoardSlotDto> Slots { get; init; } = [];
    public IReadOnlyList<SupBoardFlightDto> Flights { get; init; } = [];
}

public sealed class SupBoardSlotDto
{
    public Guid Id { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public int DayIdx { get; init; }
    public string[] Segments { get; init; } = [];
    public int Headcount { get; init; }
    public string[] FlightNos { get; init; } = [];
    public IReadOnlyList<SupBoardAssignmentDto> Assignments { get; init; } = [];
}

public sealed class SupBoardAssignmentDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string[] FlightNos { get; init; } = [];
}

public sealed class SupBoardFlightDto
{
    public Guid Id { get; init; }
    public string FlightNo { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Sta { get; init; } = string.Empty;
    public string Std { get; init; } = string.Empty;
    public bool IsDelayed { get; init; }
    public int DelayMinutes { get; init; }
}
