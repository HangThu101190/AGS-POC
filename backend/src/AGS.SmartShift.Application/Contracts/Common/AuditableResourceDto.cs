namespace AGS.SmartShift.Application.Contracts.Common;

public abstract class ResourceDto
{
    public required Guid Id { get; init; }
}

public abstract class AuditableResourceDto : ResourceDto
{
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
