using AGS.SmartShift.Application.Options;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace AGS.SmartShift.Api.HostedServices;

/// <summary>Simulates live GPS drift for active check-ins (configurable interval, default 30 min).</summary>
public sealed class MonitoringGpsJitterBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MonitoringOptions _options;
    private readonly ILogger<MonitoringGpsJitterBackgroundService> _logger;

    public MonitoringGpsJitterBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<MonitoringOptions> options,
        ILogger<MonitoringGpsJitterBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(60, _options.GpsJitterIntervalSeconds));
        _logger.LogInformation("Monitoring GPS jitter every {Seconds}s", delay.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);
                await TickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Monitoring GPS jitter tick failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceRepository>();
        var clock = scope.ServiceProvider.GetRequiredService<Application.Common.Interfaces.IDateTimeProvider>();

        var records = await attendance.ListActiveTrackedWithCoordinatesAsync(cancellationToken);
        if (records.Count == 0)
        {
            return;
        }

        var now = clock.UtcNow;
        var jitter = _options.GpsJitterDegrees;

        foreach (var record in records)
        {
            var baseLat = record.CurrentLat ?? record.CheckInLat;
            var baseLng = record.CurrentLng ?? record.CheckInLng;
            if (baseLat is null || baseLng is null)
            {
                continue;
            }

            var dLat = (Random.Shared.NextDouble() - 0.5) * 2 * jitter;
            var dLng = (Random.Shared.NextDouble() - 0.5) * 2 * jitter;
            var lat = baseLat.Value + dLat;
            var lng = baseLng.Value + dLng;

            record.ApplyGpsPosition(lat, lng, now);
            await attendance.AddGpsPointAsync(
                AttendanceGpsPoint.Create(Guid.NewGuid(), record.Id, now, lat, lng),
                cancellationToken);
        }

        await attendance.SaveChangesAsync(cancellationToken);
    }
}
