using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed record GetAttendanceByEmployeeQuery(Guid EmployeeId) : IRequest<AttendanceDto?>;

public sealed class AttendanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public DateTime CheckInUtc { get; init; }
    public DateTime? CheckOutUtc { get; init; }
    public double? CheckInLat { get; init; }
    public double? CheckInLng { get; init; }
    public double? CurrentLat { get; init; }
    public double? CurrentLng { get; init; }
    public bool InZone { get; init; }
    public string? GeoNote { get; init; }
    public bool GeoSimulated { get; init; }
    public bool IsActive { get; init; }
}
