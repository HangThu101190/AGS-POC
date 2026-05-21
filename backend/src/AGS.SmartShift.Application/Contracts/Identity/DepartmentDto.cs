using AGS.SmartShift.Application.Contracts.Common;

namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class DepartmentDto : AuditableResourceDto
{
    public required Guid SiteId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
    public required IReadOnlyList<string> AllowedRoles { get; init; }
}
