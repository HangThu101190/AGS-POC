using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Planning;

public sealed class AttendanceComplianceEvaluator
{
    public AttendanceComplianceDto? Evaluate(
        FlightCrewAssignment assignment,
        AttendanceRecord? record,
        ShiftCheckInPolicy? policy,
        Flight flight)
    {
        if (record is null)
        {
            return new AttendanceComplianceDto { Status = "not_checked_in" };
        }

        var window = BuildWindow(assignment, flight, policy);
        var checkIn = record.CheckInUtc;
        var status = "on_time";
        if (window.EarliestUtc.HasValue && checkIn < window.EarliestUtc)
        {
            status = "early";
        }
        else if (window.LatestUtc.HasValue && checkIn > window.LatestUtc)
        {
            status = "late";
        }

        if (!record.InZone)
        {
            status = "out_of_zone";
        }

        if (record.CheckOutUtc is null && window.EndUtc.HasValue && DateTime.UtcNow > window.EndUtc.Value.AddMinutes(30))
        {
            status = "missing_checkout";
        }

        return new AttendanceComplianceDto
        {
            Status = status,
            CheckInAt = record.CheckInUtc,
            CheckOutAt = record.CheckOutUtc,
            GeoNote = record.GeoNote,
            LateCheckInNote = record.LateCheckInNote,
            ValidFrom = window.ValidFromLabel,
            ValidUntil = window.ValidUntilLabel,
        };
    }

    private static (DateTime? EarliestUtc, DateTime? LatestUtc, DateTime? EndUtc, string? ValidFromLabel, string? ValidUntilLabel)
        BuildWindow(FlightCrewAssignment assignment, Flight flight, ShiftCheckInPolicy? policy)
    {
        var day = DateTime.UtcNow.Date;
        var start = assignment.WorkStart;
        var earliest = policy?.CheckInEarliestMinutesBefore ?? 30;
        var latest = policy?.CheckInLatestMinutesAfterStart ?? 15;
        var startUtc = day.Add(start.ToTimeSpan());
        return (
            startUtc.AddMinutes(-earliest),
            startUtc.AddMinutes(latest),
            day.Add(assignment.WorkEnd.ToTimeSpan()),
            $"{start:HH:mm}",
            $"{assignment.WorkEnd:HH:mm}");
    }
}
