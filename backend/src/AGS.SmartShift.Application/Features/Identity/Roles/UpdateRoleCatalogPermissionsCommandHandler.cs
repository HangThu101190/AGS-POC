using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Roles;

public sealed class UpdateRoleCatalogPermissionsCommandHandler
    : IRequestHandler<UpdateRoleCatalogPermissionsCommand, RoleCatalogDto>
{
    private readonly IRoleCatalogRepository _catalog;
    private readonly IPermissionCatalogService _permissionCatalog;
    private readonly ISender _sender;

    public UpdateRoleCatalogPermissionsCommandHandler(
        IRoleCatalogRepository catalog,
        IPermissionCatalogService permissionCatalog,
        ISender sender)
    {
        _catalog = catalog;
        _permissionCatalog = permissionCatalog;
        _sender = sender;
    }

    public async Task<RoleCatalogDto> Handle(
        UpdateRoleCatalogPermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var roles = await _catalog.ListRolesAsync(cancellationToken);
        var permissions = await _catalog.ListPermissionsAsync(cancellationToken);
        var roleCodes = roles.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var permissionCodes = permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (request.Body.Roles.Count != roleCodes.Count
            || request.Body.Roles.Keys.Any(k => !roleCodes.Contains(k)))
        {
            throw new DomainException(
                "roles_catalog_incomplete",
                "Phải gửi đầy đủ quyền cho tất cả vai trò hệ thống.");
        }

        var normalized = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (roleCode, codes) in request.Body.Roles)
        {
            var list = codes
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (list.Any(c => !permissionCodes.Contains(c)))
            {
                throw new DomainException(
                    "permission_unknown",
                    $"Mã quyền không hợp lệ cho vai trò {roleCode}.");
            }

            normalized[roleCode.Trim().ToLowerInvariant()] = list;
        }

        await _catalog.ReplaceAllRolePermissionsAsync(normalized, cancellationToken);
        _permissionCatalog.InvalidateCache();

        return await _sender.Send(new GetRoleCatalogQuery(), cancellationToken);
    }
}
