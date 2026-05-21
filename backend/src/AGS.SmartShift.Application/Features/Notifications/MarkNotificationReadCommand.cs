using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Notifications;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<NotificationDto>;
