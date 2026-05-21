using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class DeleteFlightCommandHandler : IRequestHandler<DeleteFlightCommand, Unit>
{
    private readonly IFlightRepository _flights;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IWeeklyPlanRepository _plans;

    public DeleteFlightCommandHandler(
        IFlightRepository flights,
        IFlightScheduleRepository schedules,
        IWeeklyPlanRepository plans)
    {
        _flights = flights;
        _schedules = schedules;
        _plans = plans;
    }

    public async Task<Unit> Handle(DeleteFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flights.GetByIdAsync(request.FlightId, cancellationToken)
            ?? throw new DomainException("flight_not_found", "Không tìm thấy chuyến bay.");

        var plan = await _plans.GetByWeekIdAsync(flight.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        PastDayGuard.EnsureMutableDay(flight.DayIdx, plan.TodayIdx, "xóa chuyến bay");
        await FlightScheduleGuard.EnsureMutableAsync(_schedules, flight.WeekId, "xóa chuyến bay", cancellationToken);
        await _flights.DeleteAsync(flight, cancellationToken);
        return Unit.Value;
    }
}
