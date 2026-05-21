using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Repositories;

public interface ISyncProposalRepository
{
    Task<IReadOnlyList<ShiftSyncProposal>> ListAsync(
        string? weekId,
        SyncProposalStatus? status,
        CancellationToken cancellationToken = default);

    Task<ShiftSyncProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ShiftSyncProposal?> GetPendingByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default);

    Task AddAsync(ShiftSyncProposal proposal, CancellationToken cancellationToken = default);

    Task AddRevisionAsync(ShiftRevision revision, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
