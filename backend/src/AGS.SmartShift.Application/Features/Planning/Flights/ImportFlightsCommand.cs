using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record ImportFlightsCommand(
    Stream FileStream,
    string? WeekId,
    int? DayIdx) : IRequest<FlightImportResultDto>;
