using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record SetFlightDelayCommand(
    Guid FlightId,
    int? DelayMinutes = null,
    int? EtaDelayMinutes = null,
    int? EtdDelayMinutes = null) : IRequest<FlightDto>;
