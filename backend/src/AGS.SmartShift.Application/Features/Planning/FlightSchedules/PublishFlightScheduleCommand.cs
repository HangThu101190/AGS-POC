using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.FlightSchedules;

public sealed record PublishFlightScheduleCommand(string? WeekId) : IRequest<FlightScheduleDto>;
