using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Notifications;

public sealed class ListMyNotificationsQueryHandler
    : IRequestHandler<ListMyNotificationsQuery, PagedList<NotificationDto>>
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _user;

    public ListMyNotificationsQueryHandler(
        INotificationRepository notifications,
        ICurrentUserService user)
    {
        _notifications = notifications;
        _user = user;
    }

    public async Task<PagedList<NotificationDto>> Handle(
        ListMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new DomainException("employee_required", "Tài khoản chưa liên kết nhân viên.");
        }

        var baseCriteria = request.Table.ToCriteria();
        var criteria = new TableListCriteria
        {
            Page = baseCriteria.Page,
            PageSize = baseCriteria.PageSize,
            SortBy = "createdAt",
            SortDescending = true,
            Filters = baseCriteria.Filters,
            ScopeDepartmentId = baseCriteria.ScopeDepartmentId,
        };
        var page = await _notifications.ListForEmployeeAsync(employeeId, criteria, cancellationToken);
        var items = page.Items
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAtUtc,
                UpdatedAt = n.UpdatedAtUtc,
            })
            .ToList();

        return PagedList<NotificationDto>.From(
            items,
            criteria.Page,
            criteria.EffectivePageSize,
            page.TotalCount);
    }
}
