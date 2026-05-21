using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class DailyStaffingLine : AuditableEntity<Guid>
{
    public Guid DailyStaffingPlanId { get; private set; }
    public Guid FlightId { get; private set; }
    public OperationalSegment Segment { get; private set; }
    public int SortOrder { get; private set; }
    public int TargetManning { get; private set; }
    public int ProposedManning { get; private set; }

    private DailyStaffingLine()
    {
    }

    public static DailyStaffingLine Create(
        Guid planId,
        Guid flightId,
        OperationalSegment segment,
        int sortOrder,
        int targetManning,
        int proposedManning,
        DateTime utcNow)
    {
        var line = new DailyStaffingLine
        {
            Id = Guid.NewGuid(),
            DailyStaffingPlanId = planId,
            FlightId = flightId,
            Segment = segment,
            SortOrder = sortOrder,
            TargetManning = Math.Max(0, targetManning),
            ProposedManning = Math.Max(0, proposedManning),
        };
        line.MarkCreated(utcNow);
        return line;
    }

    public void SetTargetManning(int value, DateTime utcNow)
    {
        TargetManning = Math.Max(0, value);
        MarkUpdated(utcNow);
    }

    public void SetProposedManning(int value, DateTime utcNow)
    {
        ProposedManning = Math.Max(0, value);
        MarkUpdated(utcNow);
    }
}
