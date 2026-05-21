using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class StaffingCrewProposal : AuditableEntity<Guid>
{
    public Guid DailyStaffingPlanId { get; private set; }
    public Guid StaffingLineId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? ShiftTemplateId { get; private set; }
    public CrewRole CrewRole { get; private set; }
    public TimeOnly WorkStart { get; private set; }
    public TimeOnly WorkEnd { get; private set; }
    public bool IsOvertime { get; private set; }
    public bool IsAutoAssigned { get; private set; }
    public int SortOrder { get; private set; }
    public string? Note { get; private set; }

    private StaffingCrewProposal()
    {
    }

    public static StaffingCrewProposal Create(
        Guid dailyStaffingPlanId,
        Guid staffingLineId,
        Guid employeeId,
        Guid? shiftTemplateId,
        CrewRole crewRole,
        TimeOnly workStart,
        TimeOnly workEnd,
        bool isOvertime,
        bool isAutoAssigned,
        int sortOrder,
        string? note,
        DateTime utcNow)
    {
        var proposal = new StaffingCrewProposal
        {
            Id = Guid.NewGuid(),
            DailyStaffingPlanId = dailyStaffingPlanId,
            StaffingLineId = staffingLineId,
            EmployeeId = employeeId,
            ShiftTemplateId = shiftTemplateId,
            CrewRole = crewRole,
            WorkStart = workStart,
            WorkEnd = workEnd,
            IsOvertime = isOvertime,
            IsAutoAssigned = isAutoAssigned,
            SortOrder = sortOrder,
            Note = note,
        };
        proposal.MarkCreated(utcNow);
        return proposal;
    }

    public void Update(
        CrewRole crewRole,
        TimeOnly workStart,
        TimeOnly workEnd,
        bool isOvertime,
        string? note,
        DateTime utcNow)
    {
        CrewRole = crewRole;
        WorkStart = workStart;
        WorkEnd = workEnd;
        IsOvertime = isOvertime;
        Note = note;
        MarkUpdated(utcNow);
    }
}
