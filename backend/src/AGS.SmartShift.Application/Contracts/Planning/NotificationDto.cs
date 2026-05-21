using AGS.SmartShift.Application.Contracts.Common;

namespace AGS.SmartShift.Application.Contracts.Planning;

public sealed class NotificationDto : AuditableResourceDto
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public bool IsRead { get; init; }
}
