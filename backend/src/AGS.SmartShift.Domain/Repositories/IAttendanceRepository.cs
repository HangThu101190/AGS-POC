using AGS.SmartShift.Domain.Entities.Attendance;

namespace AGS.SmartShift.Domain.Repositories;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<AttendanceRecord?> GetTrackedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> ListActiveWithCoordinatesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> ListCheckedOutAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> ListActiveTrackedWithCoordinatesAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken = default);

    Task AddGpsPointAsync(AttendanceGpsPoint point, CancellationToken cancellationToken = default);

    Task AddGpsPointsAsync(
        IReadOnlyList<AttendanceGpsPoint> points,
        CancellationToken cancellationToken = default);

    Task<DateTime?> GetMonitoringWatermarkUtcAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceGpsPoint>> ListGpsPointsAsync(
        Guid attendanceRecordId,
        int limit,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
