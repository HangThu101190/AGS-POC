using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Planning;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds a pitch-ready week: flights (from <see cref="PlanningSeedData"/>), generated slots,
/// published plan — mirrors prototype state after HCNS "Sinh slot" + "Phát hành".
/// Idempotent: skips when slots already exist for the current week.
/// </summary>
public static class DemoWorkflowSeedData
{
    private static readonly string[] PlanningDepartments = ["PVHK_DI", "PVHK_DEN", "RAMP", "BAGGAGE"];

    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        var seedTime = DateTime.UtcNow;
        var (weekId, _, todayIdx, _) = WeekCalendar.BuildCurrentWeek(seedTime.AddHours(7).Date);

        var plan = await db.WeeklyPlans
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.WeekId == weekId, cancellationToken);

        if (plan is null)
        {
            return;
        }

        if (plan.Slots.Count > 0 || await db.ShiftSlots.AnyAsync(s => s.WeeklyPlanId == plan.Id, cancellationToken))
        {
            return;
        }

        var flights = await db.Flights
            .Where(f => f.WeekId == weekId)
            .ToListAsync(cancellationToken);

        if (flights.Count == 0)
        {
            return;
        }

        var staffCounts = await db.Employees
            .AsNoTracking()
            .Where(e => e.IsActive && e.Role == UserRole.Staff)
            .Join(db.Departments, e => e.DepartmentId, d => d.Id, (e, d) => d.Code)
            .GroupBy(code => code)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count, cancellationToken);

        foreach (var dept in PlanningDepartments)
        {
            staffCounts.TryAdd(dept, 0);
        }

        var slots = ShiftSlotGenerator.GenerateForWeek(
            plan.Id,
            flights,
            PlanningDepartments,
            plan.TodayIdx,
            staffCounts);

        if (slots.Count > 0)
        {
            db.ShiftSlots.AddRange(slots);
        }

        if (plan.Status == PlanStatus.Draft && slots.Count > 0)
        {
            plan.Publish(seedTime);
        }

        var pitchFlight = flights.FirstOrDefault(f =>
            f.DayIdx == todayIdx
            && string.Equals(f.FlightNo, "VN1346", StringComparison.OrdinalIgnoreCase));

        if (pitchFlight is not null)
        {
            pitchFlight.ApplyDelay(75, seedTime);

            var hasPending = await db.ShiftSyncProposals.AnyAsync(
                p => p.FlightId == pitchFlight.Id && p.Status == SyncProposalStatus.Pending,
                cancellationToken);

            if (!hasPending)
            {
                var daySlots = slots.Where(s => s.DayIdx == pitchFlight.DayIdx).ToList();
                var slotIds = daySlots.Select(s => s.Id).ToList();
                var slotAssignments = await db.ShiftAssignments
                    .Where(a => slotIds.Contains(a.ShiftSlotId))
                    .ToListAsync(cancellationToken);

                var empIds = slotAssignments.Select(a => a.EmployeeId).Distinct().ToList();
                var employeeNames = await db.Employees
                    .AsNoTracking()
                    .Where(e => empIds.Contains(e.Id))
                    .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

                var affected = SyncProposalBuilder.BuildAffectedLines(
                    pitchFlight,
                    daySlots,
                    slotAssignments,
                    employeeNames);

                if (affected.Count > 0)
                {
                    db.ShiftSyncProposals.Add(
                        ShiftSyncProposal.CreatePending(
                            pitchFlight.Id,
                            pitchFlight.WeekId,
                            pitchFlight.DayIdx,
                            pitchFlight.FlightNo,
                            pitchFlight.DelayMinutes,
                            affected,
                            seedTime));
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
