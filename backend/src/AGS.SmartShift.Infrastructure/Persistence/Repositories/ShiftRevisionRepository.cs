using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class ShiftRevisionRepository : IShiftRevisionRepository
{
    private readonly SmartShiftDbContext _db;

    public ShiftRevisionRepository(SmartShiftDbContext db) => _db = db;

    public async Task<IReadOnlyList<ShiftRevision>> ListByAssignmentIdsAsync(
        IReadOnlyList<Guid> assignmentIds,
        CancellationToken cancellationToken = default)
    {
        if (assignmentIds.Count == 0)
        {
            return [];
        }

        return await _db.ShiftRevisions.AsNoTracking()
            .Where(r => assignmentIds.Contains(r.ShiftAssignmentId))
            .ToListAsync(cancellationToken);
    }
}
