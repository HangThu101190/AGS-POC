using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Planning;

public sealed class StaffingSupportQueries : IStaffingSupportQueries
{
    private readonly SmartShiftDbContext _db;

    public StaffingSupportQueries(SmartShiftDbContext db) => _db = db;

    public Task<IReadOnlyList<AircraftManningRule>> ListAircraftManningRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        _db.AircraftManningRules
            .AsNoTracking()
            .Where(r => r.SiteId == siteId && r.IsActive)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AircraftManningRule>)t.Result, cancellationToken);

    public Task<IReadOnlyList<AirlineManningRule>> ListAirlineManningRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        _db.AirlineManningRules
            .AsNoTracking()
            .Where(r => r.SiteId == siteId && r.IsActive)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AirlineManningRule>)t.Result, cancellationToken);

    public Task<IReadOnlyList<ShiftCheckInPolicy>> ListShiftCheckInPoliciesAsync(
        Guid siteId,
        string departmentCode,
        CancellationToken cancellationToken = default) =>
        _db.ShiftCheckInPolicies
            .AsNoTracking()
            .Where(p => p.SiteId == siteId
                        && (p.DepartmentCode == null || p.DepartmentCode == departmentCode))
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<ShiftCheckInPolicy>)t.Result, cancellationToken);

    public Task<IReadOnlyList<EmployeeDayAvailability>> ListDayAvailabilitiesAsync(
        string weekId,
        int dayIdx,
        CancellationToken cancellationToken = default) =>
        _db.EmployeeDayAvailabilities
            .AsNoTracking()
            .Where(a => a.WeekId == weekId && a.DayIdx == dayIdx)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<EmployeeDayAvailability>)t.Result, cancellationToken);

    public Task<IReadOnlyList<LeaveRequest>> ListApprovedLeaveForDayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        _db.LeaveRequests
            .AsNoTracking()
            .Where(r => r.Status == LeaveRequestStatus.Approved
                        && r.FromDate <= date
                        && r.ToDate >= date)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<LeaveRequest>)t.Result, cancellationToken);
}
