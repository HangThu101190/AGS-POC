using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Common.Notifications;

public static class ShiftLeaderNotificationHelper
{
    public static async Task NotifyShiftLeadersAsync(
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IDateTimeProvider clock,
        string type,
        string title,
        string body,
        CancellationToken cancellationToken)
    {
        var criteria = new TableListCriteria { Page = 0, PageSize = 500 };
        var page = await employees.ListAsync(criteria, cancellationToken);
        var messages = page.Items
            .Where(e => e.IsActive && (e.Role == UserRole.ShiftLeader || e.Role == UserRole.Sup))
            .Select(e => NotificationMessage.Create(e.Id, type, title, body, clock.UtcNow))
            .ToList();

        if (messages.Count > 0)
        {
            await notifications.AddRangeAsync(messages, cancellationToken);
        }
    }
}
