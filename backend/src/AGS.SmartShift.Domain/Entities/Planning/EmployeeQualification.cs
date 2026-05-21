using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class EmployeeQualification : AuditableEntity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public OperationalSegment Segment { get; private set; }
    public CrewRole CrewRole { get; private set; }
    public int Proficiency { get; private set; }
    public bool IsActive { get; private set; }

    private EmployeeQualification()
    {
    }

    public static EmployeeQualification Create(
        Guid employeeId,
        OperationalSegment segment,
        CrewRole crewRole,
        int proficiency,
        DateTime utcNow)
    {
        var qualification = new EmployeeQualification
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Segment = segment,
            CrewRole = crewRole,
            Proficiency = Math.Clamp(proficiency, 1, 3),
            IsActive = true,
        };
        qualification.MarkCreated(utcNow);
        return qualification;
    }

    public void Update(int proficiency, bool isActive, DateTime utcNow)
    {
        Proficiency = Math.Clamp(proficiency, 1, 3);
        IsActive = isActive;
        MarkUpdated(utcNow);
    }
}
