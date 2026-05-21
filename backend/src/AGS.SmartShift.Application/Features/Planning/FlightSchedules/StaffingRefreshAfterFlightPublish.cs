using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Features.Staffing;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Features.Planning.FlightSchedules;

internal static class StaffingRefreshAfterFlightPublish
{
    public const string DefaultDepartmentCode = "PVHK_DI";

    public static async Task<IReadOnlyList<int>> RefreshWeekAsync(
        Guid siteId,
        string weekId,
        IDailyStaffingRepository staffing,
        IFlightRepository flights,
        IStaffingSupportQueries support,
        IStaffingConfigRepository config,
        IDateTimeProvider clock,
        CancellationToken cancellationToken)
    {
        var staleDays = new List<int>();
        var aircraftRules = await support.ListAircraftManningRulesAsync(siteId, cancellationToken);
        var airlineRules = await support.ListAirlineManningRulesAsync(siteId, cancellationToken);
        var weekFlights = await flights.ListByWeekAsync(weekId, cancellationToken);
        var now = clock.UtcNow;

        for (var dayIdx = 0; dayIdx < 7; dayIdx++)
        {
            var dayFlights = weekFlights
                .Where(f => f.DayIdx == dayIdx && f.DepartmentCode == DefaultDepartmentCode)
                .ToList();
            if (dayFlights.Count == 0)
            {
                continue;
            }

            var plan = await staffing.GetOrCreatePlanAsync(
                siteId,
                weekId,
                dayIdx,
                DefaultDepartmentCode,
                now,
                cancellationToken);

            if (plan.Status >= DailyStaffingPlanStatus.Confirmed)
            {
                staleDays.Add(dayIdx);
                continue;
            }
            var lines = ManningProposeService.BuildLines(plan, dayFlights, aircraftRules, airlineRules, now);
            await staffing.ReplaceLinesAsync(plan.Id, lines, removeOrphanAssignments: true, cancellationToken);
            if (plan.Status < DailyStaffingPlanStatus.Proposed)
            {
                plan.SetStatus(DailyStaffingPlanStatus.Proposed, now);
            }

            await StaffingCrewAutoAssign.TryApplyAsync(
                siteId,
                weekId,
                dayIdx,
                DefaultDepartmentCode,
                plan,
                staffing,
                config,
                flights,
                support,
                clock,
                cancellationToken);
        }

        await staffing.SaveChangesAsync(cancellationToken);
        return staleDays;
    }
}
