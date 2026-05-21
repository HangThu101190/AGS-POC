using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record ListFlightsQuery(
    string? WeekId,
    int? DayIdx,
    string? DepartmentCode,
    TableListRequest Table) : IRequest<PagedList<FlightDto>>;
