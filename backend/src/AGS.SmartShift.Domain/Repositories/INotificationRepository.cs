using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface INotificationRepository
{
    Task AddRangeAsync(IReadOnlyList<NotificationMessage> messages, CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationMessage>> ListForEmployeeAsync(
        Guid employeeId,
        TableListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<NotificationMessage?> GetForEmployeeAsync(
        Guid id,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<bool> HasUnreadOfTypeAsync(
        Guid employeeId,
        string type,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
