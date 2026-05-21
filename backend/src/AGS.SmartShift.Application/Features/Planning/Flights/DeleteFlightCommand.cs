using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record DeleteFlightCommand(Guid FlightId) : IRequest<Unit>;
