using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class EmployeeDayAvailability : AuditableEntity<Guid>
{
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DayAvailabilityStatus Status { get; private set; }
    public string? Note { get; private set; }
    public bool AllowOvertime { get; private set; }

    private EmployeeDayAvailability()
    {
    }

    public static EmployeeDayAvailability Create(
        string weekId,
        int dayIdx,
        Guid employeeId,
        DayAvailabilityStatus status,
        DateTime utcNow,
        string? note = null,
        bool allowOvertime = false)
    {
        var row = new EmployeeDayAvailability
        {
            Id = Guid.NewGuid(),
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            EmployeeId = employeeId,
            Status = status,
            Note = note?.Trim(),
            AllowOvertime = allowOvertime,
        };
        row.MarkCreated(utcNow);
        return row;
    }
}
