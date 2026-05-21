using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class ListFlightsQueryHandler : IRequestHandler<ListFlightsQuery, PagedList<FlightDto>>
{
    private readonly IFlightRepository _flights;
    private readonly IPlanningWeekService _weeks;

    public ListFlightsQueryHandler(IFlightRepository flights, IPlanningWeekService weeks)
    {
        _flights = flights;
        _weeks = weeks;
    }

    public async Task<PagedList<FlightDto>> Handle(ListFlightsQuery request, CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var criteria = request.Table.ToCriteria();
        var page = await _flights.ListAsync(
            plan.WeekId,
            request.DayIdx,
            request.DepartmentCode,
            criteria,
            cancellationToken);

        var items = page.Items.Select(FlightMapping.ToDto).ToList();
        return PagedList<FlightDto>.From(
            items,
            criteria.Page,
            criteria.EffectivePageSize,
            page.TotalCount);
    }
}
