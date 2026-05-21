using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.FlightSchedules;

public sealed class GetFlightScheduleQueryHandler : IRequestHandler<GetFlightScheduleQuery, FlightScheduleDto?>
{
    private readonly IFlightScheduleRepository _schedules;

    public GetFlightScheduleQueryHandler(IFlightScheduleRepository schedules) => _schedules = schedules;

    public async Task<FlightScheduleDto?> Handle(
        GetFlightScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByWeekIdAsync(request.WeekId, cancellationToken);
        if (schedule is null)
        {
            return new FlightScheduleDto { WeekId = request.WeekId, Status = "draft", IsLocked = false };
        }

        return new FlightScheduleDto
        {
            WeekId = schedule.WeekId,
            Status = schedule.Status.ToString().ToLowerInvariant(),
            PublishedAtUtc = schedule.PublishedAtUtc,
            LockedAtUtc = schedule.LockedAtUtc,
            IsLocked = schedule.IsLocked,
        };
    }
}
