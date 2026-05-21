using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.SyncProposals;

public sealed class ListSyncProposalsQueryHandler
    : IRequestHandler<ListSyncProposalsQuery, IReadOnlyList<SyncProposalDto>>
{
    private readonly ISyncProposalRepository _proposals;

    public ListSyncProposalsQueryHandler(ISyncProposalRepository proposals) => _proposals = proposals;

    public async Task<IReadOnlyList<SyncProposalDto>> Handle(
        ListSyncProposalsQuery request,
        CancellationToken cancellationToken)
    {
        var status = request.PendingOnly ? SyncProposalStatus.Pending : (SyncProposalStatus?)null;
        var list = await _proposals.ListAsync(request.WeekId, status, cancellationToken);
        return list.Select(SyncProposalMapping.ToDto).ToList();
    }
}
