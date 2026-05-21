using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record UpdateFlightCommand(Guid FlightId, FlightUpsertDto Body) : IRequest<FlightDto>;
