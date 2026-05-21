using AGS.SmartShift.Domain.Entities.Attendance;

namespace AGS.SmartShift.Application.Features.Attendance;

internal static class AttendanceMapping
{
    public static AttendanceDto ToDto(AttendanceRecord record) =>
        new()
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            CheckInUtc = record.CheckInUtc,
            CheckOutUtc = record.CheckOutUtc,
            CheckInLat = record.CheckInLat,
            CheckInLng = record.CheckInLng,
            CurrentLat = record.CurrentLat,
            CurrentLng = record.CurrentLng,
            InZone = record.InZone,
            GeoNote = record.GeoNote,
            GeoSimulated = record.GeoSimulated,
            IsActive = record.IsActive,
        };
}
