using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed record CheckInCommand(
    double Lat,
    double Lng,
    bool InZone,
    string? GeoNote,
    bool GeoSimulated) : IRequest<AttendanceDto>;
