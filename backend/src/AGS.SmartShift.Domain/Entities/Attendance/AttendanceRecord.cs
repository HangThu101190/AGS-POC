using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Attendance;

/// <summary>Attendance with geofence coordinates for monitoring.</summary>
public sealed class AttendanceRecord : AuditableEntity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public DateTime CheckInUtc { get; private set; }
    public DateTime? CheckOutUtc { get; private set; }
    public double? CheckInLat { get; private set; }
    public double? CheckInLng { get; private set; }
    public double? CurrentLat { get; private set; }
    public double? CurrentLng { get; private set; }
    public bool InZone { get; private set; }
    public string? GeoNote { get; private set; }
    public string? LateCheckInNote { get; private set; }
    public bool GeoSimulated { get; private set; }
    public Guid? FlightCrewAssignmentId { get; private set; }
    public Guid? ShiftAssignmentId { get; private set; }

    private AttendanceRecord()
    {
    }

    public static AttendanceRecord Create(
        Guid id,
        Guid employeeId,
        DateTime checkInUtc,
        DateTime createdAtUtc)
    {
        var record = new AttendanceRecord
        {
            Id = id,
            EmployeeId = employeeId,
            CheckInUtc = checkInUtc,
        };
        record.MarkCreated(createdAtUtc);
        return record;
    }

    public bool IsActive => CheckOutUtc is null;

    public void ApplyCheckIn(
        double lat,
        double lng,
        bool inZone,
        string? geoNote,
        bool geoSimulated,
        DateTime atUtc,
        string? lateCheckInNote = null,
        Guid? flightCrewAssignmentId = null,
        Guid? shiftAssignmentId = null)
    {
        CheckInLat = lat;
        CheckInLng = lng;
        CurrentLat = lat;
        CurrentLng = lng;
        InZone = inZone;
        GeoNote = string.IsNullOrWhiteSpace(geoNote) ? null : geoNote.Trim();
        LateCheckInNote = string.IsNullOrWhiteSpace(lateCheckInNote) ? null : lateCheckInNote.Trim();
        GeoSimulated = geoSimulated;
        FlightCrewAssignmentId = flightCrewAssignmentId;
        ShiftAssignmentId = shiftAssignmentId;
        CheckInUtc = atUtc;
        CheckOutUtc = null;
        MarkUpdated(atUtc);
    }

    public void ApplyGpsPosition(double lat, double lng, DateTime atUtc, bool? inZone = null)
    {
        CurrentLat = lat;
        CurrentLng = lng;
        if (inZone.HasValue)
        {
            InZone = inZone.Value;
        }

        MarkUpdated(atUtc);
    }

    public void ApplyCheckOut(DateTime atUtc, string? earlyNote = null)
    {
        if (!string.IsNullOrWhiteSpace(earlyNote))
        {
            var note = earlyNote.Trim();
            GeoNote = string.IsNullOrWhiteSpace(GeoNote) ? $"Ra ca sớm: {note}" : $"{GeoNote} | Ra ca sớm: {note}";
        }

        CheckOutUtc = atUtc;
        MarkUpdated(atUtc);
    }
}
