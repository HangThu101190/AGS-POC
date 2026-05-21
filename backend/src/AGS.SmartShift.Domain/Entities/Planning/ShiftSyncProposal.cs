using System.Text.Json;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

/// <summary>System proposal after TBĐH delay — Shift Leader must confirm (HTML tier 4).</summary>
public sealed class ShiftSyncProposal : AuditableEntity<Guid>
{
    public Guid FlightId { get; private set; }
    public string WeekId { get; private set; } = string.Empty;
    public int DayIdx { get; private set; }
    public string FlightNo { get; private set; } = string.Empty;
    public int DelayMinutes { get; private set; }
    public SyncProposalStatus Status { get; private set; }
    public string AffectedJson { get; private set; } = "[]";
    public string Summary { get; private set; } = string.Empty;
    public DateTime? ConfirmedAtUtc { get; private set; }
    public Guid? ConfirmedByEmployeeId { get; private set; }

    private ShiftSyncProposal()
    {
    }

    public static ShiftSyncProposal CreatePending(
        Guid flightId,
        string weekId,
        int dayIdx,
        string flightNo,
        int delayMinutes,
        IReadOnlyList<SyncProposalAffectedLine> affected,
        DateTime utcNow)
    {
        var proposal = new ShiftSyncProposal
        {
            Id = Guid.NewGuid(),
            FlightId = flightId,
            WeekId = weekId.Trim(),
            DayIdx = dayIdx,
            FlightNo = flightNo.Trim().ToUpperInvariant(),
            DelayMinutes = delayMinutes,
            Status = SyncProposalStatus.Pending,
            AffectedJson = JsonSerializer.Serialize(affected),
            Summary =
                $"Đề xuất điều chỉnh ca do delay {flightNo} +{delayMinutes}' — {affected.Count} phân công bị ảnh hưởng.",
        };
        proposal.MarkCreated(utcNow);
        return proposal;
    }

    public IReadOnlyList<SyncProposalAffectedLine> GetAffected() =>
        JsonSerializer.Deserialize<List<SyncProposalAffectedLine>>(AffectedJson) ?? [];

    public void Confirm(Guid byEmployeeId, DateTime utcNow)
    {
        if (Status != SyncProposalStatus.Pending)
        {
            throw new DomainException("sync_proposal_not_pending", "Đề xuất đã được xử lý.");
        }

        Status = SyncProposalStatus.Confirmed;
        ConfirmedAtUtc = utcNow;
        ConfirmedByEmployeeId = byEmployeeId;
        MarkUpdated(utcNow);
    }

    public void Dismiss(Guid byEmployeeId, DateTime utcNow)
    {
        if (Status != SyncProposalStatus.Pending)
        {
            throw new DomainException("sync_proposal_not_pending", "Đề xuất đã được xử lý.");
        }

        Status = SyncProposalStatus.Dismissed;
        ConfirmedAtUtc = utcNow;
        ConfirmedByEmployeeId = byEmployeeId;
        MarkUpdated(utcNow);
    }
}

public sealed class SyncProposalAffectedLine
{
    public Guid SlotId { get; set; }
    public Guid? AssignmentId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string[] CurrentSegments { get; set; } = [];
    public string[] ProposedSegments { get; set; } = [];
}
