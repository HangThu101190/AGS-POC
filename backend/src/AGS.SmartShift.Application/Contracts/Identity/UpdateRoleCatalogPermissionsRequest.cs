namespace AGS.SmartShift.Application.Contracts.Identity;

/// <summary>Full role → permission codes map (replaces all grants).</summary>
public sealed class UpdateRoleCatalogPermissionsRequest
{
    public required Dictionary<string, List<string>> Roles { get; init; }
}
