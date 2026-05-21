using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Features.SyncProposals;

internal static class SyncProposalMapping
{
    public static SyncProposalDto ToDto(ShiftSyncProposal proposal) => new()
    {
        Id = proposal.Id,
        FlightId = proposal.FlightId,
        WeekId = proposal.WeekId,
        DayIdx = proposal.DayIdx,
        FlightNo = proposal.FlightNo,
        DelayMinutes = proposal.DelayMinutes,
        Status = proposal.Status.ToString().ToLowerInvariant(),
        Summary = proposal.Summary,
        Affected = proposal.GetAffected()
            .Select(a => new SyncProposalAffectedDto
            {
                SlotId = a.SlotId,
                AssignmentId = a.AssignmentId,
                EmployeeName = a.EmployeeName,
                CurrentSegments = a.CurrentSegments,
                ProposedSegments = a.ProposedSegments,
            })
            .ToList(),
    };
}
