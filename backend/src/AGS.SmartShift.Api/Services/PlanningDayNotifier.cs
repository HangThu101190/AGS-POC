using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AGS.SmartShift.Api.Services;

public sealed class PlanningDayNotifier : IPlanningDayNotifier
{
    private readonly IHubContext<PlanningHub> _hub;

    public PlanningDayNotifier(IHubContext<PlanningHub> hub) => _hub = hub;

    public Task NotifyFlightsDayUpdatedAsync(
        string weekId,
        int dayIdx,
        string change,
        IReadOnlyList<Guid>? flightIds = null,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(PlanningHub.DayGroup(weekId, dayIdx))
            .SendAsync(
                "flightsDayUpdated",
                new { weekId, dayIdx, change, flightIds },
                cancellationToken);

    public Task NotifyStaffingDayUpdatedAsync(
        string weekId,
        int dayIdx,
        string change,
        string planStatus,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(PlanningHub.DayGroup(weekId, dayIdx))
            .SendAsync(
                "staffingDayUpdated",
                new { weekId, dayIdx, change, planStatus },
                cancellationToken);

    public Task NotifyAttendanceUpdatedAsync(
        string weekId,
        int dayIdx,
        Guid employeeId,
        Guid? flightCrewAssignmentId = null,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(PlanningHub.DayGroup(weekId, dayIdx))
            .SendAsync(
                "attendanceUpdated",
                new { weekId, dayIdx, employeeId, flightCrewAssignmentId },
                cancellationToken);

    public Task NotifyImportProgressAsync(
        string weekId,
        int dayIdx,
        Guid jobId,
        int progressPercent,
        string status,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(PlanningHub.DayGroup(weekId, dayIdx))
            .SendAsync(
                "importProgress",
                new { weekId, dayIdx, jobId, progressPercent, status },
                cancellationToken);

    public Task NotifyFlightSchedulePublishedAsync(
        string weekId,
        IReadOnlyList<int> staleDayIndices,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(PlanningHub.WeekGroup(weekId))
            .SendAsync(
                "flightSchedulePublished",
                new { weekId, staleDayIndices },
                cancellationToken);
}
