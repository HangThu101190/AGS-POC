namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class RoleCatalogDto
{
    public required IReadOnlyList<RoleCatalogRoleDto> Roles { get; init; }
    public required IReadOnlyList<RoleCatalogPermissionDto> Permissions { get; init; }
}

public sealed class RoleCatalogRoleDto
{
    public required string Code { get; init; }
    public required string NameVi { get; init; }
    public required string NameEn { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
}

public sealed class RoleCatalogPermissionDto
{
    public required string Code { get; init; }
    public required string Module { get; init; }
    public required string NameVi { get; init; }
    public required string NameEn { get; init; }
}
