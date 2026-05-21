using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Planning;

public sealed record CrewAssignmentEngineInput(
    DailyStaffingPlan Plan,
    IReadOnlyList<DailyStaffingLine> Lines,
    IReadOnlyList<Flight> Flights,
    IReadOnlyList<ShiftTemplate> ShiftTemplates,
    IReadOnlyList<Employee> Employees,
    IReadOnlyList<EmployeeQualification> Qualifications,
    IReadOnlyList<EmployeeDayAvailability> Availabilities,
    IReadOnlyList<FlightCrewAssignment> ExistingAssignments,
    DateTime UtcNow);

public static class CrewAssignmentEngine
{
    public static AutoAssignStaffingResultDto Run(CrewAssignmentEngineInput input)
    {
        var warnings = new List<string>();
        var proposals = new List<StaffingCrewProposal>();
        var flightById = input.Flights.ToDictionary(f => f.Id);
        var employeeById = input.Employees.ToDictionary(e => e.Id);
        var qualsByEmployee = input.Qualifications
            .Where(q => q.IsActive)
            .GroupBy(q => q.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var availByEmployee = input.Availabilities.ToDictionary(a => a.EmployeeId);
        var assignedEmployees = input.ExistingAssignments.Select(a => a.EmployeeId).ToHashSet();
        var hoursByEmployee = new Dictionary<Guid, decimal>();
        var templates = input.ShiftTemplates.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToList();
        var otCount = 0;
        var unfilled = 0;
        var sort = 0;

        foreach (var line in input.Lines.OrderBy(l => l.SortOrder))
        {
            if (!flightById.TryGetValue(line.FlightId, out var flight))
            {
                continue;
            }

            var existingOnLine = input.ExistingAssignments.Count(a => a.StaffingLineId == line.Id);
            var needed = Math.Max(0, line.TargetManning - existingOnLine);
            if (needed == 0)
            {
                continue;
            }

            var template = PickTemplate(templates, line.Segment, flight);
            var window = ServiceWindow(flight);
            var candidates = RankCandidates(
                input.Employees,
                qualsByEmployee,
                availByEmployee,
                line.Segment,
                CrewRole.General,
                assignedEmployees,
                hoursByEmployee,
                template?.MaxHours ?? 8m);

            var filled = 0;
            foreach (var employee in candidates)
            {
                if (filled >= needed)
                {
                    break;
                }

                var isOt = IsOvertimeAssignment(availByEmployee, employee.Id);
                if (isOt)
                {
                    otCount++;
                }

                var workStart = template?.StartTime ?? window.Start;
                var workEnd = template?.EndTime ?? window.End;
                proposals.Add(StaffingCrewProposal.Create(
                    input.Plan.Id,
                    line.Id,
                    employee.Id,
                    template?.Id,
                    CrewRole.General,
                    workStart,
                    workEnd,
                    isOt,
                    isAutoAssigned: true,
                    sortOrder: sort++,
                    isOt ? "OT - tự động phân công" : null,
                    input.UtcNow));

                assignedEmployees.Add(employee.Id);
                hoursByEmployee[employee.Id] = hoursByEmployee.GetValueOrDefault(employee.Id) + ShiftHours(workStart, workEnd);
                filled++;
            }

            if (filled < needed)
            {
                unfilled += needed - filled;
                warnings.Add($"Chuyến {flight.FlightNo} thiếu {needed - filled} nhân sự.");
            }

            TryAssignRole(
                input, line, flight, templates, qualsByEmployee, availByEmployee, employeeById,
                assignedEmployees, hoursByEmployee, ref proposals, ref sort, ref otCount,
                CrewRole.Counter, OperationalSegment.Qn);
            TryAssignRole(
                input, line, flight, templates, qualsByEmployee, availByEmployee, employeeById,
                assignedEmployees, hoursByEmployee, ref proposals, ref sort, ref otCount,
                CrewRole.Gate, OperationalSegment.Qn);
            if (line.Segment == OperationalSegment.Qt)
            {
                TryAssignRole(
                    input, line, flight, templates, qualsByEmployee, availByEmployee, employeeById,
                    assignedEmployees, hoursByEmployee, ref proposals, ref sort, ref otCount,
                    CrewRole.Sup, OperationalSegment.Qt);
            }
        }

        var totalEmployees = assignedEmployees.Count;
        var totalSlots = input.Lines.Sum(l => l.TargetManning);
        var filledSlots = totalSlots - unfilled;
        var coverage = totalSlots > 0 ? Math.Round(100m * filledSlots / totalSlots, 1) : 100m;

        return new AutoAssignStaffingResultDto
        {
            Proposals = MapProposals(proposals, employeeById, templates),
            Warnings = warnings,
            Summary = new AutoAssignSummaryDto
            {
                TotalEmployees = totalEmployees,
                TotalOvertime = otCount,
                CoveragePercent = coverage,
                UnfilledSlots = unfilled,
            },
        };
    }

    private static void TryAssignRole(
        CrewAssignmentEngineInput input,
        DailyStaffingLine line,
        Flight flight,
        IReadOnlyList<ShiftTemplate> templates,
        Dictionary<Guid, List<EmployeeQualification>> qualsByEmployee,
        Dictionary<Guid, EmployeeDayAvailability> availByEmployee,
        Dictionary<Guid, Employee> employeeById,
        HashSet<Guid> assignedEmployees,
        Dictionary<Guid, decimal> hoursByEmployee,
        ref List<StaffingCrewProposal> proposals,
        ref int sort,
        ref int otCount,
        CrewRole role,
        OperationalSegment segment)
    {
        if (proposals.Any(p => p.StaffingLineId == line.Id && p.CrewRole == role))
        {
            return;
        }

        var template = PickTemplate(templates, segment, flight);
        var window = ServiceWindow(flight);
        var candidates = RankCandidates(
            input.Employees,
            qualsByEmployee,
            availByEmployee,
            segment,
            role,
            assignedEmployees,
            hoursByEmployee,
            template?.MaxHours ?? 8m);

        var employee = candidates.FirstOrDefault();
        if (employee is null)
        {
            return;
        }

        var isOt = IsOvertimeAssignment(availByEmployee, employee.Id);
        if (isOt)
        {
            otCount++;
        }

        proposals.Add(StaffingCrewProposal.Create(
            input.Plan.Id,
            line.Id,
            employee.Id,
            template?.Id,
            role,
            template?.StartTime ?? window.Start,
            template?.EndTime ?? window.End,
            isOt,
            true,
            sort++,
            null,
            input.UtcNow));
        assignedEmployees.Add(employee.Id);
    }

    private static IReadOnlyList<Employee> RankCandidates(
        IReadOnlyList<Employee> employees,
        Dictionary<Guid, List<EmployeeQualification>> qualsByEmployee,
        Dictionary<Guid, EmployeeDayAvailability> availByEmployee,
        OperationalSegment segment,
        CrewRole role,
        HashSet<Guid> assignedEmployees,
        Dictionary<Guid, decimal> hoursByEmployee,
        decimal maxHours)
    {
        return employees
            .Where(e => e.IsActive && !assignedEmployees.Contains(e.Id))
            .Where(e => IsQualified(qualsByEmployee, e.Id, segment, role))
            .OrderByDescending(e => Proficiency(qualsByEmployee, e.Id, segment, role))
            .ThenBy(e => hoursByEmployee.GetValueOrDefault(e.Id))
            .ThenBy(e => IsOvertimeAssignment(availByEmployee, e.Id) ? 1 : 0)
            .ThenBy(e => e.Code)
            .Where(e => CanTakeShift(availByEmployee, e.Id, maxHours, hoursByEmployee))
            .ToList();
    }

    private static bool IsQualified(
        Dictionary<Guid, List<EmployeeQualification>> qualsByEmployee,
        Guid employeeId,
        OperationalSegment segment,
        CrewRole role)
    {
        if (!qualsByEmployee.TryGetValue(employeeId, out var list))
        {
            return role == CrewRole.General;
        }

        return list.Any(q =>
            q.Segment == segment
            && (q.CrewRole == role || (role == CrewRole.General && q.CrewRole != CrewRole.Sup)));
    }

    private static int Proficiency(
        Dictionary<Guid, List<EmployeeQualification>> qualsByEmployee,
        Guid employeeId,
        OperationalSegment segment,
        CrewRole role) =>
        qualsByEmployee.TryGetValue(employeeId, out var list)
            ? list.Where(q => q.Segment == segment && q.CrewRole == role).Select(q => q.Proficiency).DefaultIfEmpty(1).Max()
            : 1;

    private static bool CanTakeShift(
        Dictionary<Guid, EmployeeDayAvailability> availByEmployee,
        Guid employeeId,
        decimal maxHours,
        Dictionary<Guid, decimal> hoursByEmployee)
    {
        if (availByEmployee.TryGetValue(employeeId, out var avail)
            && avail.Status != DayAvailabilityStatus.Available
            && !avail.AllowOvertime)
        {
            return false;
        }

        return hoursByEmployee.GetValueOrDefault(employeeId) < maxHours;
    }

    private static bool IsOvertimeAssignment(
        Dictionary<Guid, EmployeeDayAvailability> availByEmployee,
        Guid employeeId) =>
        availByEmployee.TryGetValue(employeeId, out var avail)
        && avail.Status != DayAvailabilityStatus.Available;

    private static ShiftTemplate? PickTemplate(
        IReadOnlyList<ShiftTemplate> templates,
        OperationalSegment segment,
        Flight flight)
    {
        var segmentTemplates = templates.Where(t => t.Segment == null || t.Segment == segment).ToList();
        return segmentTemplates.FirstOrDefault()
            ?? templates.FirstOrDefault();
    }

    private static (TimeOnly Start, TimeOnly End) ServiceWindow(Flight flight)
    {
        var sta = ParseTime(flight.Sta, new TimeOnly(6, 0));
        var std = ParseTime(flight.Std, sta.AddHours(2));
        var start = sta.AddMinutes(-30);
        var end = std.AddMinutes(15);
        return (start, end);
    }

    private static decimal ShiftHours(TimeOnly start, TimeOnly end)
    {
        var span = end.ToTimeSpan() - start.ToTimeSpan();
        if (span.TotalHours < 0)
        {
            span += TimeSpan.FromHours(24);
        }

        return (decimal)span.TotalHours;
    }

    private static TimeOnly ParseTime(string value, TimeOnly fallback)
    {
        if (TimeOnly.TryParse(value.Trim(), out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static IReadOnlyList<StaffingCrewProposalDto> MapProposals(
        IReadOnlyList<StaffingCrewProposal> proposals,
        Dictionary<Guid, Employee> employeeById,
        IReadOnlyList<ShiftTemplate> templates)
    {
        var templateById = templates.ToDictionary(t => t.Id);
        return proposals.Select(p =>
        {
            employeeById.TryGetValue(p.EmployeeId, out var emp);
            templateById.TryGetValue(p.ShiftTemplateId ?? Guid.Empty, out var template);
            return new StaffingCrewProposalDto
            {
                Id = p.Id,
                StaffingLineId = p.StaffingLineId,
                EmployeeId = p.EmployeeId,
                EmployeeCode = emp?.Code ?? string.Empty,
                EmployeeName = emp?.Name ?? string.Empty,
                ShiftTemplateId = p.ShiftTemplateId,
                ShiftTemplateCode = template?.Code,
                CrewRole = p.CrewRole,
                WorkStart = p.WorkStart.ToString("HH:mm"),
                WorkEnd = p.WorkEnd.ToString("HH:mm"),
                IsOvertime = p.IsOvertime,
                IsAutoAssigned = p.IsAutoAssigned,
                Note = p.Note,
            };
        }).ToList();
    }
}
