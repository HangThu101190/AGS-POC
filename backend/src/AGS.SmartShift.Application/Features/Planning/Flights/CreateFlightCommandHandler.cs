using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, FlightDto>
{
    private readonly IFlightRepository _flights;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IPlanningWeekService _weeks;
    private readonly IDateTimeProvider _clock;

    public CreateFlightCommandHandler(
        IFlightRepository flights,
        IFlightScheduleRepository schedules,
        IPlanningWeekService weeks,
        IDateTimeProvider clock)
    {
        _flights = flights;
        _schedules = schedules;
        _weeks = weeks;
        _clock = clock;
    }

    public async Task<FlightDto> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var body = request.Body;
        var plan = await _weeks.GetOrCreateWeekAsync(body.WeekId, cancellationToken);
        PastDayGuard.EnsureMutableDay(body.DayIdx, plan.TodayIdx, "thêm chuyến bay");
        await FlightScheduleGuard.EnsureMutableForDayAsync(
            _schedules,
            plan.WeekId,
            body.DayIdx,
            plan.TodayIdx,
            "thêm chuyến bay",
            cancellationToken);

        var flight = Flight.Create(
            plan.SiteId,
            plan.WeekId,
            body.DayIdx,
            body.FlightNo,
            body.DepartureFlightNo,
            body.Route,
            body.Sta,
            body.Std,
            body.DepartmentCode,
            body.Manning,
            _clock.UtcNow,
            body.Aircraft,
            body.ManningExplain,
            body.IsVip,
            body.VipNote,
            body.Gate,
            body.Belt,
            body.Parking,
            body.Remark);

        await _flights.AddAsync(flight, cancellationToken);
        return FlightMapping.ToDto(flight);
    }
}
