using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly SmartShiftDbContext _db;

    public NotificationRepository(SmartShiftDbContext db) => _db = db;

    public async Task AddRangeAsync(
        IReadOnlyList<NotificationMessage> messages,
        CancellationToken cancellationToken = default)
    {
        await _db.NotificationMessages.AddRangeAsync(messages, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<PagedResult<NotificationMessage>> ListForEmployeeAsync(
        Guid employeeId,
        TableListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var q = _db.NotificationMessages.AsNoTracking()
            .Where(n => n.EmployeeId == employeeId);

        if (criteria.Filters != null
            && criteria.Filters.TryGetValue("unreadOnly", out var unread)
            && string.Equals(unread, "true", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(n => !n.IsRead);
        }

        q = criteria.SortDescending
            ? q.OrderByDescending(n => n.CreatedAtUtc)
            : q.OrderBy(n => n.CreatedAtUtc);

        return q.ToPagedListAsync(criteria, static (query, _, _) => query, cancellationToken);
    }

    public Task<NotificationMessage?> GetForEmployeeAsync(
        Guid id,
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        _db.NotificationMessages.FirstOrDefaultAsync(
            n => n.Id == id && n.EmployeeId == employeeId,
            cancellationToken);

    public Task<bool> HasUnreadOfTypeAsync(
        Guid employeeId,
        string type,
        CancellationToken cancellationToken = default) =>
        _db.NotificationMessages.AsNoTracking()
            .AnyAsync(
                n => n.EmployeeId == employeeId && !n.IsRead && n.Type == type,
                cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
