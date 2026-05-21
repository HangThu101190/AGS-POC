namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class DepartmentUpsertDto
{
    public Guid? SiteId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public IReadOnlyList<string>? AllowedRoles { get; init; }
}
