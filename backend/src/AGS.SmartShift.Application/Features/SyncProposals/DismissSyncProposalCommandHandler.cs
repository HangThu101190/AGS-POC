using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.SyncProposals;

public sealed class DismissSyncProposalCommandHandler : IRequestHandler<DismissSyncProposalCommand, SyncProposalDto>
{
    private readonly ISyncProposalRepository _proposals;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public DismissSyncProposalCommandHandler(
        ISyncProposalRepository proposals,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _proposals = proposals;
        _user = user;
        _clock = clock;
    }

    public async Task<SyncProposalDto> Handle(
        DismissSyncProposalCommand request,
        CancellationToken cancellationToken)
    {
        var proposal = await _proposals.GetByIdAsync(request.ProposalId, cancellationToken)
            ?? throw new DomainException("sync_proposal_not_found", "Không tìm thấy đề xuất sync ca.");

        var byEmpId = _user.EmployeeId ?? Guid.Empty;
        proposal.Dismiss(byEmpId, _clock.UtcNow);
        await _proposals.SaveChangesAsync(cancellationToken);
        return SyncProposalMapping.ToDto(proposal);
    }
}
