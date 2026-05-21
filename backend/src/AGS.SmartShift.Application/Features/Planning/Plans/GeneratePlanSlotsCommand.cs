using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed record GeneratePlanSlotsCommand(string? WeekId) : IRequest<WeeklyPlanDto>;
