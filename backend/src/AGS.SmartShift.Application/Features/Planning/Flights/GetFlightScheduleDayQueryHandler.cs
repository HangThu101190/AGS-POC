using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class GetFlightScheduleDayQueryHandler
    : IRequestHandler<GetFlightScheduleDayQuery, FlightScheduleDayDto?>
{
    private readonly IFlightScheduleDayRepository _days;
    private readonly IPlanningWeekService _weeks;

    public GetFlightScheduleDayQueryHandler(
        IFlightScheduleDayRepository days,
        IPlanningWeekService weeks)
    {
        _days = days;
        _weeks = weeks;
    }

    public async Task<FlightScheduleDayDto?> Handle(
        GetFlightScheduleDayQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var day = await _days.GetAsync(plan.WeekId, request.DayIdx, cancellationToken);
        if (day is null)
        {
            return null;
        }

        return new FlightScheduleDayDto
        {
            WeekId = day.WeekId,
            DayIdx = day.DayIdx,
            SourceDayLabel = day.SourceDayLabel,
            SheetRemark = day.SheetRemark,
        };
    }
}
