using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IStaffingSupportQueries
{
    Task<IReadOnlyList<AircraftManningRule>> ListAircraftManningRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AirlineManningRule>> ListAirlineManningRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShiftCheckInPolicy>> ListShiftCheckInPoliciesAsync(
        Guid siteId,
        string departmentCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDayAvailability>> ListDayAvailabilitiesAsync(
        string weekId,
        int dayIdx,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequest>> ListApprovedLeaveForDayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}
