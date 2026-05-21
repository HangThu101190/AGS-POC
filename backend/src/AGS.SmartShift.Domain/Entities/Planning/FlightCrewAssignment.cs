using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class FlightCrewAssignment : AuditableEntity<Guid>
{
    public Guid StaffingLineId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public CrewRole Role { get; private set; }
    public TimeOnly WorkStart { get; private set; }
    public TimeOnly WorkEnd { get; private set; }
    public bool IsOvertime { get; private set; }
    public int SortOrder { get; private set; }

    private FlightCrewAssignment()
    {
    }

    public static FlightCrewAssignment Create(
        Guid staffingLineId,
        Guid employeeId,
        CrewRole role,
        TimeOnly workStart,
        TimeOnly workEnd,
        bool isOvertime,
        int sortOrder,
        DateTime utcNow)
    {
        var assignment = new FlightCrewAssignment
        {
            Id = Guid.NewGuid(),
            StaffingLineId = staffingLineId,
            EmployeeId = employeeId,
            Role = role,
            WorkStart = workStart,
            WorkEnd = workEnd,
            IsOvertime = isOvertime,
            SortOrder = sortOrder,
        };
        assignment.MarkCreated(utcNow);
        return assignment;
    }

    public void Update(
        CrewRole role,
        TimeOnly workStart,
        TimeOnly workEnd,
        bool isOvertime,
        DateTime utcNow)
    {
        Role = role;
        WorkStart = workStart;
        WorkEnd = workEnd;
        IsOvertime = isOvertime;
        MarkUpdated(utcNow);
    }
}
