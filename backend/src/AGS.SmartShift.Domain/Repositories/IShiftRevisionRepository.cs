using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IShiftRevisionRepository
{
    Task<IReadOnlyList<ShiftRevision>> ListByAssignmentIdsAsync(
        IReadOnlyList<Guid> assignmentIds,
        CancellationToken cancellationToken = default);
}
