using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed record PublishWeeklyPlanCommand(string? WeekId) : IRequest<WeeklyPlanDto>;
