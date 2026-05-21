using System.Text.Json;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class EnqueueFlightImportCommandHandler : IRequestHandler<EnqueueFlightImportCommand, FlightImportJobDto>
{
    private readonly IFlightImportJobRepository _jobs;
    private readonly IPlanningWeekService _weeks;
    private readonly IFlightImportQueue _queue;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public EnqueueFlightImportCommandHandler(
        IFlightImportJobRepository jobs,
        IPlanningWeekService weeks,
        IFlightImportQueue queue,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _jobs = jobs;
        _weeks = weeks;
        _queue = queue;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<FlightImportJobDto> Handle(EnqueueFlightImportCommand request, CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dayIdx = request.DayIdx ?? plan.TodayIdx;
        PastDayGuard.EnsureMutableDay(dayIdx, plan.TodayIdx, "import lịch bay");

        var userId = _currentUser.UserAccountId ?? throw new UnauthorizedAccessException();
        var job = FlightImportJob.Create(plan.SiteId, plan.WeekId, dayIdx, userId, _clock.UtcNow);
        await _jobs.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await _jobs.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _queue.EnqueueAsync(
            new FlightImportWorkItem(job.Id, plan.WeekId, request.DayIdx, request.FileBytes),
            cancellationToken).ConfigureAwait(false);

        return Map(job, null);
    }

    internal static FlightImportJobDto Map(FlightImportJob job, FlightImportResultDto? result) =>
        new()
        {
            Id = job.Id,
            WeekId = job.WeekId,
            DayIdx = job.DayIdx,
            Status = job.Status,
            ProgressPercent = job.ProgressPercent,
            ErrorMessage = job.ErrorMessage,
            Result = result,
        };

    internal static FlightImportResultDto? TryParseResult(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<FlightImportResultDto>(json);
    }
}
