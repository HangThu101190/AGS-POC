using AGS.SmartShift.Domain.Entities.Leave;

namespace AGS.SmartShift.Domain.Repositories;

public interface ILeaveRequestRepository
{
    Task<IReadOnlyList<LeaveRequest>> ListRecentAsync(int limit = 200, CancellationToken cancellationToken = default);

    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveType>> ListLeaveTypesAsync(CancellationToken cancellationToken = default);
}
