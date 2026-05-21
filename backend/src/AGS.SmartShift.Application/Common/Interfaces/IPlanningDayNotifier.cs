namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IPlanningDayNotifier
{
    Task NotifyFlightsDayUpdatedAsync(
        string weekId,
        int dayIdx,
        string change,
        IReadOnlyList<Guid>? flightIds = null,
        CancellationToken cancellationToken = default);

    Task NotifyStaffingDayUpdatedAsync(
        string weekId,
        int dayIdx,
        string change,
        string planStatus,
        CancellationToken cancellationToken = default);

    Task NotifyAttendanceUpdatedAsync(
        string weekId,
        int dayIdx,
        Guid employeeId,
        Guid? flightCrewAssignmentId = null,
        CancellationToken cancellationToken = default);

    Task NotifyImportProgressAsync(
        string weekId,
        int dayIdx,
        Guid jobId,
        int progressPercent,
        string status,
        CancellationToken cancellationToken = default);

    Task NotifyFlightSchedulePublishedAsync(
        string weekId,
        IReadOnlyList<int> staleDayIndices,
        CancellationToken cancellationToken = default);
}
