using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.SyncProposals;

public sealed record ListSyncProposalsQuery(string? WeekId, bool PendingOnly = true)
    : IRequest<IReadOnlyList<SyncProposalDto>>;
