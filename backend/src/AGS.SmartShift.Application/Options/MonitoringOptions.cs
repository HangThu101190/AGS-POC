namespace AGS.SmartShift.Application.Options;

public sealed class MonitoringOptions
{
    public const string SectionName = "Monitoring";

    /// <summary>GPS jitter interval (default 30 min = 60×30 s).</summary>
    public int GpsJitterIntervalSeconds { get; set; } = 1800;

    public double GpsJitterDegrees { get; set; } = 0.00022;

    /// <summary>SignalR <c>snapshotUpdated</c> broadcast interval.</summary>
    public int SignalRBroadcastIntervalSeconds { get; set; } = 30;
}
