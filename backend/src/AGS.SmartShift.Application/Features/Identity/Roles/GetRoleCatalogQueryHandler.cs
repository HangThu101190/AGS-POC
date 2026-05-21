using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Roles;

public sealed class GetRoleCatalogQueryHandler : IRequestHandler<GetRoleCatalogQuery, RoleCatalogDto>
{
    private readonly IRoleCatalogRepository _catalog;

    public GetRoleCatalogQueryHandler(IRoleCatalogRepository catalog) => _catalog = catalog;

    public async Task<RoleCatalogDto> Handle(GetRoleCatalogQuery request, CancellationToken cancellationToken)
    {
        var roles = await _catalog.ListRolesAsync(cancellationToken);
        var permissions = await _catalog.ListPermissionsAsync(cancellationToken);
        var grants = await _catalog.ListRolePermissionsAsync(cancellationToken);

        var grantsByRole = grants
            .GroupBy(g => g.RoleCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionCode).ToList(), StringComparer.OrdinalIgnoreCase);

        return new RoleCatalogDto
        {
            Roles = roles
                .Select(r => new RoleCatalogRoleDto
                {
                    Code = r.Code,
                    NameVi = r.NameVi,
                    NameEn = r.NameEn,
                    Description = r.Description,
                    Permissions = grantsByRole.TryGetValue(r.Code, out var codes)
                        ? codes
                        : Array.Empty<string>(),
                })
                .ToList(),
            Permissions = permissions
                .Select(p => new RoleCatalogPermissionDto
                {
                    Code = p.Code,
                    Module = p.Module,
                    NameVi = p.NameVi,
                    NameEn = p.NameEn,
                })
                .ToList(),
        };
    }
}
