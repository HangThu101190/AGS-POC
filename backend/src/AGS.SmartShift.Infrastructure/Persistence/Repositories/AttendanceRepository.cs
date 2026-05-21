using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly SmartShiftDbContext _db;

    public AttendanceRepository(SmartShiftDbContext db) => _db = db;

    public Task<AttendanceRecord?> GetByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        _db.AttendanceRecords.AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId, cancellationToken);

    public Task<AttendanceRecord?> GetTrackedByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        _db.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId, cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListActiveWithCoordinatesAsync(
        CancellationToken cancellationToken = default) =>
        await _db.AttendanceRecords.AsNoTracking()
            .Where(a => a.CheckOutUtc == null && a.CurrentLat != null && a.CurrentLng != null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListActiveTrackedWithCoordinatesAsync(
        CancellationToken cancellationToken = default) =>
        await _db.AttendanceRecords
            .Where(a => a.CheckOutUtc == null && a.CurrentLat != null && a.CurrentLng != null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListCheckedOutAsync(
        CancellationToken cancellationToken = default) =>
        await _db.AttendanceRecords.AsNoTracking()
            .Where(a => a.CheckOutUtc != null)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        await _db.AttendanceRecords.AddAsync(record, cancellationToken);
    }

    public async Task AddGpsPointAsync(AttendanceGpsPoint point, CancellationToken cancellationToken = default)
    {
        await _db.AttendanceGpsPoints.AddAsync(point, cancellationToken);
    }

    public async Task AddGpsPointsAsync(
        IReadOnlyList<AttendanceGpsPoint> points,
        CancellationToken cancellationToken = default)
    {
        if (points.Count == 0)
        {
            return;
        }

        await _db.AttendanceGpsPoints.AddRangeAsync(points, cancellationToken);
    }

    public async Task<DateTime?> GetMonitoringWatermarkUtcAsync(CancellationToken cancellationToken = default)
    {
        var attMax = await _db.AttendanceRecords.AsNoTracking()
            .MaxAsync(a => (DateTime?)a.UpdatedAtUtc, cancellationToken);
        var gpsMax = await _db.AttendanceGpsPoints.AsNoTracking()
            .MaxAsync(p => (DateTime?)p.AtUtc, cancellationToken);
        if (attMax is null)
        {
            return gpsMax;
        }

        if (gpsMax is null)
        {
            return attMax;
        }

        return attMax > gpsMax ? attMax : gpsMax;
    }

    public async Task<IReadOnlyList<AttendanceGpsPoint>> ListGpsPointsAsync(
        Guid attendanceRecordId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await _db.AttendanceGpsPoints.AsNoTracking()
            .Where(p => p.AttendanceRecordId == attendanceRecordId)
            .OrderByDescending(p => p.AtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
