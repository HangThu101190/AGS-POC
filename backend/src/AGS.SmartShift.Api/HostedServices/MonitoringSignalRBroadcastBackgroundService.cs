using AGS.SmartShift.Api.Hubs;
using AGS.SmartShift.Application.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AGS.SmartShift.Api.HostedServices;

/// <summary>Broadcasts <c>snapshotUpdated</c> to monitoring subscribers on an interval (default 30s).</summary>
public sealed class MonitoringSignalRBroadcastBackgroundService : BackgroundService
{
    private readonly IHubContext<MonitoringHub> _hub;
    private readonly MonitoringOptions _options;
    private readonly ILogger<MonitoringSignalRBroadcastBackgroundService> _logger;

    public MonitoringSignalRBroadcastBackgroundService(
        IHubContext<MonitoringHub> hub,
        IOptions<MonitoringOptions> options,
        ILogger<MonitoringSignalRBroadcastBackgroundService> logger)
    {
        _hub = hub;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(5, _options.SignalRBroadcastIntervalSeconds));
        _logger.LogInformation("Monitoring SignalR broadcast every {Seconds}s", delay.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);
                await _hub.Clients
                    .Group(MonitoringHub.GroupName)
                    .SendAsync("snapshotUpdated", cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Monitoring SignalR broadcast failed");
            }
        }
    }
}
