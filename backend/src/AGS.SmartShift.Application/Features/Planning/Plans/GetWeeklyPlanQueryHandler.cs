using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed class GetWeeklyPlanQueryHandler : IRequestHandler<GetWeeklyPlanQuery, WeeklyPlanDto>
{
    private readonly IPlanningWeekService _weeks;

    public GetWeeklyPlanQueryHandler(IPlanningWeekService weeks) => _weeks = weeks;

    public async Task<WeeklyPlanDto> Handle(GetWeeklyPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        return PlanMapping.ToDto(plan);
    }
}
