using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Roles;

public sealed record GetRoleCatalogQuery : IRequest<RoleCatalogDto>;
