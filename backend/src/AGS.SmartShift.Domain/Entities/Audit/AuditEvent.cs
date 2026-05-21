namespace AGS.SmartShift.Domain.Entities.Audit;

public sealed class AuditEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid? UserAccountId { get; private set; }
    public string? EmployeeCode { get; private set; }
    public string HttpMethod { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }

    private AuditEvent()
    {
    }

    public static AuditEvent Create(
        DateTime occurredAtUtc,
        Guid? userAccountId,
        string? employeeCode,
        string httpMethod,
        string path,
        int statusCode)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = occurredAtUtc,
            UserAccountId = userAccountId,
            EmployeeCode = employeeCode,
            HttpMethod = httpMethod,
            Path = path.Length > 500 ? path[..500] : path,
            StatusCode = statusCode,
        };
    }
}
