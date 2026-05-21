using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Api.HostedServices;

/// <summary>Removes expired and revoked refresh tokens (daily).</summary>
public sealed class RefreshTokenCleanupBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupBackgroundService> _logger;

    public RefreshTokenCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token cleanup failed");
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var accounts = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();
        var removed = await accounts.RemoveExpiredRefreshTokensAsync(clock.UtcNow, cancellationToken);
        if (removed > 0)
        {
            _logger.LogInformation("Removed {Count} expired/revoked refresh tokens", removed);
        }
    }
}
