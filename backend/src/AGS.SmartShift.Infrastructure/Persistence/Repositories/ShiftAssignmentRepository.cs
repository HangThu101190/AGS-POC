using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class ShiftAssignmentRepository : IShiftAssignmentRepository
{
    private readonly SmartShiftDbContext _db;

    public ShiftAssignmentRepository(SmartShiftDbContext db) => _db = db;

    public async Task<IReadOnlyList<ShiftAssignment>> ListBySlotIdsAsync(
        IReadOnlyCollection<Guid> slotIds,
        CancellationToken cancellationToken = default)
    {
        if (slotIds.Count == 0)
        {
            return Array.Empty<ShiftAssignment>();
        }

        return await _db.ShiftAssignments
            .Where(a => slotIds.Contains(a.ShiftSlotId))
            .ToListAsync(cancellationToken);
    }

    public Task<ShiftAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ShiftAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(ShiftAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _db.ShiftAssignments.AddAsync(assignment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(ShiftAssignment assignment, CancellationToken cancellationToken = default)
    {
        _db.ShiftAssignments.Remove(assignment);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
