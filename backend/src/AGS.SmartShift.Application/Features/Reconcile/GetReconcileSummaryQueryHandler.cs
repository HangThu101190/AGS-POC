using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Reconcile;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed class GetReconcileSummaryQueryHandler : IRequestHandler<GetReconcileSummaryQuery, ReconcileSummaryDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IShiftRevisionRepository _revisions;
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IAttendanceRepository _attendance;

    public GetReconcileSummaryQueryHandler(
        IPlanningWeekService weeks,
        IWeeklyPlanRepository plans,
        IShiftAssignmentRepository assignments,
        IShiftRevisionRepository revisions,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IAttendanceRepository attendance)
    {
        _weeks = weeks;
        _plans = plans;
        _assignments = assignments;
        _revisions = revisions;
        _employees = employees;
        _departments = departments;
        _attendance = attendance;
    }

    public async Task<ReconcileSummaryDto> Handle(
        GetReconcileSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var tracked = await _plans.GetWithSlotsAsync(plan.WeekId, cancellationToken);
        var weekDates = tracked?.GetWeekDates() ?? plan.GetWeekDates();
        var deptCode = request.DepartmentCode.Trim().ToUpperInvariant();

        var deptPage = await _departments.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 50 },
            cancellationToken);
        var department = deptPage.Items.FirstOrDefault(d => d.Code == deptCode)
            ?? throw new DomainException("dept_not_found", "Không tìm thấy phòng ban.");

        var empPage = await _employees.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 200 },
            cancellationToken);
        var deptEmployees = empPage.Items
            .Where(e => e.IsActive && e.DepartmentId == department.Id)
            .ToList();

        var deptSlots = tracked?.Slots.Where(s => s.DepartmentCode == deptCode).ToList() ?? [];
        var slotIds = deptSlots.Select(s => s.Id).ToList();
        var allAssignments = await _assignments.ListBySlotIdsAsync(slotIds, cancellationToken);
        var assignmentIds = allAssignments.Select(a => a.Id).ToList();
        var revisionList = await _revisions.ListByAssignmentIdsAsync(assignmentIds, cancellationToken);
        var revisionByAssignment = revisionList
            .GroupBy(r => r.ShiftAssignmentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAtUtc).First());

        var rows = new List<ReconcileEmployeeDto>();
        foreach (var emp in deptEmployees)
        {
            var empAssignments = allAssignments.Where(a => a.EmployeeId == emp.Id).ToList();
            if (empAssignments.Count == 0)
            {
                continue;
            }

            var att = await _attendance.GetByEmployeeIdAsync(emp.Id, cancellationToken);
            var days = new List<ReconcileDayDto>();
            var hasFlag = false;

            for (var d = 0; d < 7; d++)
            {
                var dateLabel = weekDates.ElementAtOrDefault(d) ?? $"D{d}";
                var dayAssignments = empAssignments
                    .Where(a => deptSlots.Any(s => s.Id == a.ShiftSlotId && s.DayIdx == d))
                    .ToList();

                if (dayAssignments.Count == 0)
                {
                    continue;
                }

                var plannedSegments = dayAssignments
                    .SelectMany(a => deptSlots.First(s => s.Id == a.ShiftSlotId).GetSegments())
                    .Distinct()
                    .ToList();

                var planned = AttendanceCodeEngine.FromSegments(plannedSegments, dateLabel);

                var hasRevision = dayAssignments.Any(a => revisionByAssignment.ContainsKey(a.Id));
                ShiftRevision? rev = dayAssignments
                    .Select(a => revisionByAssignment.GetValueOrDefault(a.Id))
                    .FirstOrDefault(r => r is not null);

                var hasCheckIn = att is not null && d == plan.TodayIdx && att.CheckInUtc.Date <= DateTime.UtcNow.Date;
                IReadOnlyList<string> actualSegments;
                if (hasRevision && rev is not null)
                {
                    actualSegments = rev.GetDeltaSegments();
                }
                else if (hasCheckIn)
                {
                    actualSegments = plannedSegments;
                }
                else
                {
                    actualSegments = [];
                }

                var actual = AttendanceCodeEngine.FromSegments(actualSegments, dateLabel);
                var mismatch = planned.Code != actual.Code || hasRevision;
                if (mismatch)
                {
                    hasFlag = true;
                }

                days.Add(new ReconcileDayDto
                {
                    DayIdx = d,
                    DateLabel = dateLabel,
                    PlannedCode = planned.Code,
                    ActualCode = actual.Code,
                    CodeType = actual.Type,
                    Mismatch = mismatch,
                    HasRevision = hasRevision,
                    HasCheckIn = hasCheckIn,
                    PlannedTotalHours = planned.TotalHours,
                    ActualTotalHours = actual.TotalHours,
                    PlannedSegments = plannedSegments.ToArray(),
                    ActualSegments = actualSegments.ToArray(),
                    RevisionReason = rev?.Reason,
                    RevisionBy = rev?.ByCode,
                });
            }

            if (days.Count == 0)
            {
                continue;
            }

            rows.Add(new ReconcileEmployeeDto
            {
                EmployeeId = emp.Id,
                Code = emp.Code,
                Name = emp.Name,
                Days = days,
                HasFlag = hasFlag,
            });
        }

        return new ReconcileSummaryDto
        {
            WeekId = plan.WeekId,
            DepartmentCode = deptCode,
            Employees = rows,
        };
    }
}
