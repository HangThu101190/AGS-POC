using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record EnqueueFlightImportCommand(
    byte[] FileBytes,
    string? WeekId,
    int? DayIdx) : IRequest<FlightImportJobDto>;
