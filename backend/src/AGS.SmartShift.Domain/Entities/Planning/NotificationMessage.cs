using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class NotificationMessage : AuditableEntity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    private NotificationMessage()
    {
    }

    public static NotificationMessage Create(
        Guid employeeId,
        string type,
        string title,
        string body,
        DateTime utcNow)
    {
        var message = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Type = type.Trim(),
            Title = title.Trim(),
            Body = body.Trim(),
            IsRead = false,
        };
        message.MarkCreated(utcNow);
        return message;
    }

    public void MarkAsRead(DateTime utcNow)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        MarkUpdated(utcNow);
    }
}
