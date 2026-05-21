namespace AGS.SmartShift.Domain.Common;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : struct, IEquatable<TId>
{
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    protected void MarkCreated(DateTime utcNow) => CreatedAtUtc = utcNow;

    protected void MarkUpdated(DateTime utcNow) => UpdatedAtUtc = utcNow;
}
