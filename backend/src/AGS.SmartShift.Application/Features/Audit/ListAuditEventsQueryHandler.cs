using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Audit;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Audit;

public sealed class ListAuditEventsQueryHandler : IRequestHandler<ListAuditEventsQuery, PagedList<AuditEventDto>>
{
    private readonly IAuditEventRepository _audit;

    public ListAuditEventsQueryHandler(IAuditEventRepository audit) => _audit = audit;

    public async Task<PagedList<AuditEventDto>> Handle(
        ListAuditEventsQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = request.Table.ToCriteria();
        var page = await _audit.ListAsync(criteria, request.FromUtc, request.ToUtc, cancellationToken);
        var items = page.Items.Select(row => new AuditEventDto
        {
            Id = row.Event.Id,
            OccurredAtUtc = row.Event.OccurredAtUtc,
            EmployeeCode = row.Event.EmployeeCode,
            ActorName = row.ActorName,
            HttpMethod = row.Event.HttpMethod,
            Path = row.Event.Path,
            StatusCode = row.Event.StatusCode,
        }).ToList();

        return PagedList<AuditEventDto>.From(
            items,
            criteria.Page,
            criteria.EffectivePageSize,
            page.TotalCount);
    }
}
