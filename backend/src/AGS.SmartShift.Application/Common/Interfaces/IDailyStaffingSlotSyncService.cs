using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IDailyStaffingSlotSyncService
{
    Task<(int SlotsUpdated, int AssignmentsCreated)> SyncAsync(
        string weekId,
        int dayIdx,
        string departmentCode,
        IReadOnlyList<FlightCrewAssignment> crewAssignments,
        IReadOnlyDictionary<Guid, Guid> lineIdToFlightId,
        IReadOnlyDictionary<Guid, Flight> flightsById,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
