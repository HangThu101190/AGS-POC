using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;

namespace AGS.SmartShift.Application.Features.Staffing;

internal static class StaffingCrewAutoAssign
{
    public static async Task<AutoAssignStaffingResultDto?> TryApplyAsync(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        DailyStaffingPlan plan,
        IDailyStaffingRepository staffing,
        IStaffingConfigRepository config,
        IFlightRepository flights,
        IStaffingSupportQueries support,
        IDateTimeProvider clock,
        CancellationToken cancellationToken)
    {
        if (plan.IsLocked || plan.Status >= DailyStaffingPlanStatus.Confirmed)
        {
            return null;
        }

        var dept = departmentCode.Trim().ToUpperInvariant();
        var lines = await staffing.ListLinesAsync(plan.Id, cancellationToken);
        if (lines.Count == 0)
        {
            return null;
        }

        var dayFlights = (await flights.ListByWeekAsync(weekId, cancellationToken))
            .Where(f => f.DayIdx == dayIdx && f.DepartmentCode == dept)
            .ToList();
        if (dayFlights.Count == 0)
        {
            return null;
        }

        var department = await config.FindDepartmentByCodeAsync(siteId, dept, cancellationToken);
        var employees = department is null
            ? []
            : await config.ListActiveEmployeesByDepartmentIdAsync(department.Id, cancellationToken);
        var qualifications = department is null
            ? []
            : await config.ListQualificationsForDepartmentAsync(department.Id, cancellationToken);
        var templates = await config.ListShiftTemplatesAsync(siteId, dept, cancellationToken);
        var avail = await support.ListDayAvailabilitiesAsync(weekId, dayIdx, cancellationToken);
        var existing = await staffing.ListAssignmentsForPlanAsync(plan.Id, cancellationToken);
        var now = clock.UtcNow;

        var result = CrewAssignmentEngine.Run(new CrewAssignmentEngineInput(
            plan,
            lines,
            dayFlights,
            templates,
            employees,
            qualifications,
            avail,
            existing,
            now));

        var proposalEntities = result.Proposals.Select(dto => StaffingCrewProposal.Create(
            plan.Id,
            dto.StaffingLineId,
            dto.EmployeeId,
            dto.ShiftTemplateId,
            dto.CrewRole,
            TimeOnly.Parse(dto.WorkStart),
            TimeOnly.Parse(dto.WorkEnd),
            dto.IsOvertime,
            dto.IsAutoAssigned,
            0,
            dto.Note,
            now)).ToList();

        await staffing.ReplaceProposalsAsync(plan.Id, proposalEntities, cancellationToken);
        if (proposalEntities.Count > 0)
        {
            var assignments = StaffingWorkflowHelpers.CopyProposalsToAssignments(proposalEntities, now);
            await staffing.ReplaceAssignmentsForPlanAsync(plan.Id, assignments, cancellationToken);
        }

        if (plan.Status < DailyStaffingPlanStatus.CrewDraft)
        {
            plan.SetStatus(DailyStaffingPlanStatus.CrewDraft, now);
        }

        return result;
    }
}
