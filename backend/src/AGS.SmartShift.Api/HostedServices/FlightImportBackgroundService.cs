using System.Text.Json;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Features.Planning.Flights;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Api.HostedServices;

public sealed class FlightImportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IFlightImportQueue _queue;
    private readonly ILogger<FlightImportBackgroundService> _logger;

    public FlightImportBackgroundService(
        IServiceProvider services,
        IFlightImportQueue queue,
        ILogger<FlightImportBackgroundService> logger)
    {
        _services = services;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await ProcessItemAsync(item, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flight import job {JobId} failed", item.JobId);
            }
        }
    }

    private async Task ProcessItemAsync(FlightImportWorkItem item, CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IFlightImportJobRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var notifier = scope.ServiceProvider.GetRequiredService<IPlanningDayNotifier>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var job = await jobs.GetByIdAsync(item.JobId, cancellationToken).ConfigureAwait(false);
        if (job is null)
        {
            return;
        }

        job.MarkRunning(10, clock.UtcNow);
        await jobs.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await notifier.NotifyImportProgressAsync(job.WeekId, job.DayIdx, job.Id, 10, "running", cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await using var stream = new MemoryStream(item.FileBytes);
            var result = await mediator
                .Send(new ImportFlightsCommand(stream, item.WeekId, item.DayIdx), cancellationToken)
                .ConfigureAwait(false);

            var json = JsonSerializer.Serialize(result);
            job.MarkCompleted(json, clock.UtcNow);
            await jobs.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await notifier.NotifyImportProgressAsync(job.WeekId, job.DayIdx, job.Id, 100, "completed", cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            job.MarkFailed(ex.Message, clock.UtcNow);
            await jobs.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await notifier.NotifyImportProgressAsync(job.WeekId, job.DayIdx, job.Id, 0, "failed", cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }
}
