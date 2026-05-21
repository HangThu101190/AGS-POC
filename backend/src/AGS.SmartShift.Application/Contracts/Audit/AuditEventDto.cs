namespace AGS.SmartShift.Application.Contracts.Audit;

public sealed class AuditEventDto
{
    public Guid Id { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public string? EmployeeCode { get; init; }
    public string? ActorName { get; init; }
    public string HttpMethod { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int StatusCode { get; init; }
}
