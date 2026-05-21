using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class DailyStaffingRepository : IDailyStaffingRepository
{
    private readonly SmartShiftDbContext _db;

    public DailyStaffingRepository(SmartShiftDbContext db) => _db = db;

    public Task<DailyStaffingPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default) =>
        _db.DailyStaffingPlans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

    public Task<DailyStaffingPlan?> GetPlanAsync(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        CancellationToken cancellationToken = default) =>
        _db.DailyStaffingPlans.FirstOrDefaultAsync(
            p => p.SiteId == siteId
                 && p.WeekId == weekId
                 && p.DayIdx == dayIdx
                 && p.DepartmentCode == departmentCode,
            cancellationToken);

    public async Task<DailyStaffingPlan> GetOrCreatePlanAsync(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetPlanAsync(siteId, weekId, dayIdx, departmentCode, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var plan = DailyStaffingPlan.Create(siteId, weekId, dayIdx, departmentCode, utcNow);
        await _db.DailyStaffingPlans.AddAsync(plan, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public Task<IReadOnlyList<DailyStaffingLine>> ListLinesAsync(
        Guid planId,
        CancellationToken cancellationToken = default) =>
        _db.DailyStaffingLines
            .Where(l => l.DailyStaffingPlanId == planId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<DailyStaffingLine>)t.Result, cancellationToken);

    public Task<DailyStaffingLine?> GetLineByIdAsync(Guid lineId, CancellationToken cancellationToken = default) =>
        _db.DailyStaffingLines.FirstOrDefaultAsync(l => l.Id == lineId, cancellationToken);

    public async Task<IReadOnlyList<FlightCrewAssignment>> ListAssignmentsForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var lineIds = await _db.DailyStaffingLines
            .Where(l => l.DailyStaffingPlanId == planId)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);
        return await _db.FlightCrewAssignments
            .Where(a => lineIds.Contains(a.StaffingLineId))
            .ToListAsync(cancellationToken);
    }

    public Task<FlightCrewAssignment?> GetAssignmentByIdAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default) =>
        _db.FlightCrewAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

    public Task<IReadOnlyList<StaffingCrewProposal>> ListProposalsForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default) =>
        _db.StaffingCrewProposals
            .Where(p => p.DailyStaffingPlanId == planId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<StaffingCrewProposal>)t.Result, cancellationToken);

    public async Task ReplaceProposalsAsync(
        Guid planId,
        IReadOnlyList<StaffingCrewProposal> proposals,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.StaffingCrewProposals
            .Where(p => p.DailyStaffingPlanId == planId)
            .ToListAsync(cancellationToken);
        _db.StaffingCrewProposals.RemoveRange(existing);
        if (proposals.Count > 0)
        {
            await _db.StaffingCrewProposals.AddRangeAsync(proposals, cancellationToken);
        }
    }

    public Task<DailyStaffingBioHeader?> GetBioHeaderAsync(
        Guid planId,
        CancellationToken cancellationToken = default) =>
        _db.DailyStaffingBioHeaders.FirstOrDefaultAsync(h => h.DailyStaffingPlanId == planId, cancellationToken);

    public async Task<DailyStaffingBioHeader> GetOrCreateBioHeaderAsync(
        Guid planId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetBioHeaderAsync(planId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var header = DailyStaffingBioHeader.Create(planId, utcNow);
        await _db.DailyStaffingBioHeaders.AddAsync(header, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return header;
    }

    public async Task ReplaceLinesAsync(
        Guid planId,
        IReadOnlyList<DailyStaffingLine> lines,
        bool removeOrphanAssignments,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.DailyStaffingLines
            .Where(l => l.DailyStaffingPlanId == planId)
            .ToListAsync(cancellationToken);
        if (removeOrphanAssignments && existing.Count > 0)
        {
            var lineIds = existing.Select(l => l.Id).ToList();
            var assignments = await _db.FlightCrewAssignments
                .Where(a => lineIds.Contains(a.StaffingLineId))
                .ToListAsync(cancellationToken);
            _db.FlightCrewAssignments.RemoveRange(assignments);
        }

        _db.DailyStaffingLines.RemoveRange(existing);
        if (lines.Count > 0)
        {
            await _db.DailyStaffingLines.AddRangeAsync(lines, cancellationToken);
        }
    }

    public async Task ReplaceAssignmentsForPlanAsync(
        Guid planId,
        IReadOnlyList<FlightCrewAssignment> assignments,
        CancellationToken cancellationToken = default)
    {
        var lineIds = await _db.DailyStaffingLines
            .Where(l => l.DailyStaffingPlanId == planId)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);
        var existing = await _db.FlightCrewAssignments
            .Where(a => lineIds.Contains(a.StaffingLineId))
            .ToListAsync(cancellationToken);
        _db.FlightCrewAssignments.RemoveRange(existing);
        if (assignments.Count > 0)
        {
            await _db.FlightCrewAssignments.AddRangeAsync(assignments, cancellationToken);
        }
    }

    public async Task AddAssignmentAsync(FlightCrewAssignment assignment, CancellationToken cancellationToken = default) =>
        await _db.FlightCrewAssignments.AddAsync(assignment, cancellationToken);

    public Task RemoveAssignmentAsync(FlightCrewAssignment assignment, CancellationToken cancellationToken = default)
    {
        _db.FlightCrewAssignments.Remove(assignment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
