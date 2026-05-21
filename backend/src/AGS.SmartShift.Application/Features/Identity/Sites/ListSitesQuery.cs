using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Sites;

public sealed record ListSitesQuery : IRequest<IReadOnlyList<SiteDto>>;
