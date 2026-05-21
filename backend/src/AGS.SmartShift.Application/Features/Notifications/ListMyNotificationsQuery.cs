using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Notifications;

public sealed record ListMyNotificationsQuery(TableListRequest Table) : IRequest<PagedList<NotificationDto>>;
