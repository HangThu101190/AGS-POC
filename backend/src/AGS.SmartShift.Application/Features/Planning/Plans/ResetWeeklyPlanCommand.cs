using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed record ResetWeeklyPlanCommand(string? WeekId) : IRequest<WeeklyPlanDto>;
