using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IDailyStaffingRepository
{
    Task<DailyStaffingPlan?> GetPlanAsync(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        CancellationToken cancellationToken = default);

    Task<DailyStaffingPlan> GetOrCreatePlanAsync(
        Guid siteId,
        string weekId,
        int dayIdx,
        string departmentCode,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyStaffingLine>> ListLinesAsync(
        Guid planId,
        CancellationToken cancellationToken = default);

    Task<DailyStaffingLine?> GetLineByIdAsync(Guid lineId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlightCrewAssignment>> ListAssignmentsForPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default);

    Task<FlightCrewAssignment?> GetAssignmentByIdAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task ReplaceLinesAsync(
        Guid planId,
        IReadOnlyList<DailyStaffingLine> lines,
        bool removeOrphanAssignments,
        CancellationToken cancellationToken = default);

    Task AddAssignmentAsync(FlightCrewAssignment assignment, CancellationToken cancellationToken = default);

    Task RemoveAssignmentAsync(FlightCrewAssignment assignment, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
