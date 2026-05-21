namespace AGS.SmartShift.Domain.Entities.Attendance;

/// <summary>GPS trail point for monitoring history (tree grid).</summary>
public sealed class AttendanceGpsPoint
{
    public Guid Id { get; private set; }
    public Guid AttendanceRecordId { get; private set; }
    public DateTime AtUtc { get; private set; }
    public double Lat { get; private set; }
    public double Lng { get; private set; }

    private AttendanceGpsPoint()
    {
    }

    public static AttendanceGpsPoint Create(
        Guid id,
        Guid attendanceRecordId,
        DateTime atUtc,
        double lat,
        double lng) =>
        new()
        {
            Id = id,
            AttendanceRecordId = attendanceRecordId,
            AtUtc = atUtc,
            Lat = lat,
            Lng = lng,
        };
}
