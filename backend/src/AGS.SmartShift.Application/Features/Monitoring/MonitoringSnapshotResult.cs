namespace AGS.SmartShift.Application.Features.Monitoring;

public sealed class MonitoringSnapshotResult
{
    public bool Unchanged { get; init; }
    public MonitoringSnapshotDto? Snapshot { get; init; }

    public static MonitoringSnapshotResult FromSnapshot(MonitoringSnapshotDto snapshot) =>
        new() { Unchanged = false, Snapshot = snapshot };

    public static MonitoringSnapshotResult UnchangedResult() =>
        new() { Unchanged = true };
}
