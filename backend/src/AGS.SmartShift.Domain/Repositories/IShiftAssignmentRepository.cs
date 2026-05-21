using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IShiftAssignmentRepository
{
    Task<IReadOnlyList<ShiftAssignment>> ListBySlotIdsAsync(
        IReadOnlyCollection<Guid> slotIds,
        CancellationToken cancellationToken = default);

    Task<ShiftAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(ShiftAssignment assignment, CancellationToken cancellationToken = default);

    Task RemoveAsync(ShiftAssignment assignment, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
