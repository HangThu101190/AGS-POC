using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Sites;

public sealed class ListSitesQueryHandler : IRequestHandler<ListSitesQuery, IReadOnlyList<SiteDto>>
{
    private readonly ISiteRepository _sites;

    public ListSitesQueryHandler(ISiteRepository sites) => _sites = sites;

    public async Task<IReadOnlyList<SiteDto>> Handle(ListSitesQuery request, CancellationToken cancellationToken)
    {
        var sites = await _sites.ListAsync(cancellationToken);
        return sites
            .Select(s => new SiteDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Timezone = s.Timezone,
                CreatedAt = s.CreatedAtUtc,
                UpdatedAt = s.UpdatedAtUtc,
            })
            .ToList();
    }
}
