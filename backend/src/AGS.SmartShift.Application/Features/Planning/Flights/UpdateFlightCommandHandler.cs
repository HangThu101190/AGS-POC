using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class UpdateFlightCommandHandler : IRequestHandler<UpdateFlightCommand, FlightDto>
{
    private readonly IFlightRepository _flights;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IDateTimeProvider _clock;

    public UpdateFlightCommandHandler(
        IFlightRepository flights,
        IFlightScheduleRepository schedules,
        IWeeklyPlanRepository plans,
        IDateTimeProvider clock)
    {
        _flights = flights;
        _schedules = schedules;
        _plans = plans;
        _clock = clock;
    }

    public async Task<FlightDto> Handle(UpdateFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flights.GetByIdAsync(request.FlightId, cancellationToken)
            ?? throw new DomainException("flight_not_found", "Không tìm thấy chuyến bay.");

        var plan = await _plans.GetByWeekIdAsync(flight.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        PastDayGuard.EnsureMutableDay(flight.DayIdx, plan.TodayIdx, "sửa chuyến bay");
        await FlightScheduleGuard.EnsureMutableForDayAsync(
            _schedules,
            flight.WeekId,
            flight.DayIdx,
            plan.TodayIdx,
            "sửa chuyến bay",
            cancellationToken);

        var body = request.Body;
        flight.UpdateDetails(
            body.Route,
            body.Sta,
            body.Std,
            body.DepartmentCode,
            body.Manning,
            _clock.UtcNow,
            body.DepartureFlightNo,
            body.Aircraft,
            body.ManningExplain,
            body.IsVip,
            body.VipNote,
            body.Gate,
            body.Belt,
            body.Parking,
            body.Remark);

        await _flights.SaveChangesAsync(cancellationToken);
        return FlightMapping.ToDto(flight);
    }
}
