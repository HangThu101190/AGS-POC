using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed record GetFlightImportJobQuery(Guid JobId) : IRequest<FlightImportJobDto?>;
