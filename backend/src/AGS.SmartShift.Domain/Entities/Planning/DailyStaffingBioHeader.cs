using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class DailyStaffingBioHeader : AuditableEntity<Guid>
{
    public Guid DailyStaffingPlanId { get; private set; }
    public Guid? MorningSupEmployeeId { get; private set; }
    public Guid? EveningSupEmployeeId { get; private set; }
    public string? RadioNote { get; private set; }
    public Guid? AssignerEmployeeId { get; private set; }

    private DailyStaffingBioHeader()
    {
    }

    public static DailyStaffingBioHeader Create(Guid dailyStaffingPlanId, DateTime utcNow)
    {
        var header = new DailyStaffingBioHeader
        {
            Id = Guid.NewGuid(),
            DailyStaffingPlanId = dailyStaffingPlanId,
        };
        header.MarkCreated(utcNow);
        return header;
    }

    public void Update(
        Guid? morningSupEmployeeId,
        Guid? eveningSupEmployeeId,
        string? radioNote,
        Guid? assignerEmployeeId,
        DateTime utcNow)
    {
        MorningSupEmployeeId = morningSupEmployeeId;
        EveningSupEmployeeId = eveningSupEmployeeId;
        RadioNote = radioNote;
        AssignerEmployeeId = assignerEmployeeId;
        MarkUpdated(utcNow);
    }
}
