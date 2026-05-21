using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Notifications;

public sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, NotificationDto>
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notifications,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _notifications = notifications;
        _user = user;
        _clock = clock;
    }

    public async Task<NotificationDto> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new DomainException("employee_required", "Tài khoản chưa liên kết nhân viên.");
        }

        var message = await _notifications.GetForEmployeeAsync(
                request.NotificationId,
                employeeId,
                cancellationToken)
            ?? throw new DomainException("notification_not_found", "Không tìm thấy thông báo.");

        message.MarkAsRead(_clock.UtcNow);
        await _notifications.SaveChangesAsync(cancellationToken);

        return new NotificationDto
        {
            Id = message.Id,
            Type = message.Type,
            Title = message.Title,
            Body = message.Body,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAtUtc,
            UpdatedAt = message.UpdatedAtUtc,
        };
    }
}
