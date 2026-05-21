using AGS.SmartShift.Application.Contracts.Common;

namespace AGS.SmartShift.Application.Contracts.Identity;

public sealed class SiteDto : AuditableResourceDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Timezone { get; init; }
}
