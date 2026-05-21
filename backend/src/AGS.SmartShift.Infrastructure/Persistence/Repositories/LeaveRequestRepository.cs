using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly SmartShiftDbContext _db;

    public LeaveRequestRepository(SmartShiftDbContext db) => _db = db;

    public Task<IReadOnlyList<LeaveRequest>> ListRecentAsync(int limit = 200, CancellationToken cancellationToken = default) =>
        _db.LeaveRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<LeaveRequest>)t.Result, cancellationToken);

    public Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default) =>
        await _db.LeaveRequests.AddAsync(request, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);

    public Task<IReadOnlyList<LeaveType>> ListLeaveTypesAsync(CancellationToken cancellationToken = default) =>
        _db.LeaveTypes
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Code)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<LeaveType>)t.Result, cancellationToken);
}
