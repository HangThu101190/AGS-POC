using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AGS.SmartShift.Infrastructure.Services;

public sealed class PermissionCatalogService : IPermissionCatalogService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly object _gate = new();
    private Dictionary<string, List<string>>? _byRole;

    public PermissionCatalogService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(
        string roleCode,
        CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        var key = roleCode.Trim().ToLowerInvariant();
        lock (_gate)
        {
            return _byRole!.TryGetValue(key, out var list) ? list : Array.Empty<string>();
        }
    }

    public void InvalidateCache()
    {
        lock (_gate)
        {
            _byRole = null;
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_byRole is not null)
        {
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var catalog = scope.ServiceProvider.GetRequiredService<IRoleCatalogRepository>();
        var grants = await catalog.ListRolePermissionsAsync(cancellationToken);
        var map = grants
            .GroupBy(g => g.RoleCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.PermissionCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);

        lock (_gate)
        {
            _byRole ??= map;
        }
    }
}
