using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Plans;

public sealed class GeneratePlanSlotsCommandHandler : IRequestHandler<GeneratePlanSlotsCommand, WeeklyPlanDto>
{
    private static readonly string[] PlanningDepartments = ["PVHK_DI", "PVHK_DEN", "RAMP", "BAGGAGE"];

    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IFlightRepository _flights;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public GeneratePlanSlotsCommandHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IFlightRepository flights,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _plans = plans;
        _flights = flights;
        _employees = employees;
        _clock = clock;
    }

    public async Task<WeeklyPlanDto> Handle(
        GeneratePlanSlotsCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var weekFlights = await _flights.ListByWeekAsync(plan.WeekId, cancellationToken);
        var staffCounts = await _employees.CountActiveStaffByDepartmentCodeAsync(cancellationToken);
        var newSlots = ShiftSlotGenerator.GenerateForWeek(
            plan.Id,
            weekFlights,
            PlanningDepartments,
            plan.TodayIdx,
            staffCounts);

        await _plans.ReplaceFutureSlotsAsync(
            plan.WeekId,
            newSlots,
            plan.TodayIdx,
            _clock.UtcNow,
            cancellationToken);

        var refreshed = await _plans.GetWithSlotsAsync(plan.WeekId, cancellationToken) ?? plan;
        return PlanMapping.ToDto(refreshed);
    }
}
