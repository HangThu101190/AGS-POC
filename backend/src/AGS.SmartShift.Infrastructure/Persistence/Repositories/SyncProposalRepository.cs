using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class SyncProposalRepository : ISyncProposalRepository
{
    private readonly SmartShiftDbContext _db;

    public SyncProposalRepository(SmartShiftDbContext db) => _db = db;

    public async Task<IReadOnlyList<ShiftSyncProposal>> ListAsync(
        string? weekId,
        SyncProposalStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ShiftSyncProposals.AsQueryable();
        if (!string.IsNullOrWhiteSpace(weekId))
        {
            query = query.Where(p => p.WeekId == weekId);
        }

        if (status is not null)
        {
            query = query.Where(p => p.Status == status);
        }

        return await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<ShiftSyncProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ShiftSyncProposals.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ShiftSyncProposal?> GetPendingByFlightIdAsync(
        Guid flightId,
        CancellationToken cancellationToken = default) =>
        _db.ShiftSyncProposals.FirstOrDefaultAsync(
            p => p.FlightId == flightId && p.Status == SyncProposalStatus.Pending,
            cancellationToken);

    public async Task AddAsync(ShiftSyncProposal proposal, CancellationToken cancellationToken = default)
    {
        await _db.ShiftSyncProposals.AddAsync(proposal, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRevisionAsync(ShiftRevision revision, CancellationToken cancellationToken = default)
    {
        await _db.ShiftRevisions.AddAsync(revision, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
