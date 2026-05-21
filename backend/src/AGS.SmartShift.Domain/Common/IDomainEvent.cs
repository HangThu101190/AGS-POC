namespace AGS.SmartShift.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
