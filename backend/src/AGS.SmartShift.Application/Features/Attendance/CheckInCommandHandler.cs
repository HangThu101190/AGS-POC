using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using AGS.SmartShift.Domain.Services;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed class CheckInCommandHandler : IRequestHandler<CheckInCommand, AttendanceDto>
{
    private readonly IAttendanceRepository _attendance;
    private readonly IWorkZoneRepository _workZones;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public CheckInCommandHandler(
        IAttendanceRepository attendance,
        IWorkZoneRepository workZones,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _attendance = attendance;
        _workZones = workZones;
        _user = user;
        _clock = clock;
    }

    public async Task<AttendanceDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new ForbiddenAccessException("Staff account required for check-in.");
        }

        var polygon = await ResolvePolygonAsync(cancellationToken);
        var serverInZone = GeofenceService.Contains(request.Lat, request.Lng, polygon);
        if (!serverInZone && string.IsNullOrWhiteSpace(request.GeoNote))
        {
            throw new DomainException(
                "outside_zone_note_required",
                "Ngoài vùng làm việc — cần ghi chú lý do trước khi check-in.");
        }

        var now = _clock.UtcNow;
        var record = await _attendance.GetTrackedByEmployeeIdAsync(employeeId, cancellationToken);

        if (record is not null && record.IsActive)
        {
            throw new DomainException("already_checked_in", "Already checked in for the current shift.");
        }

        if (record is null)
        {
            record = AttendanceRecord.Create(Guid.NewGuid(), employeeId, now, now);
            record.ApplyCheckIn(
                request.Lat,
                request.Lng,
                serverInZone,
                request.GeoNote,
                request.GeoSimulated,
                now);
            await _attendance.AddAsync(record, cancellationToken);
        }
        else
        {
            record.ApplyCheckIn(
                request.Lat,
                request.Lng,
                serverInZone,
                request.GeoNote,
                request.GeoSimulated,
                now);
        }

        await _attendance.AddGpsPointAsync(
            AttendanceGpsPoint.Create(Guid.NewGuid(), record.Id, now, request.Lat, request.Lng),
            cancellationToken);

        await _attendance.SaveChangesAsync(cancellationToken);

        return AttendanceMapping.ToDto(record);
    }

    private async Task<IReadOnlyList<(double Lat, double Lng)>> ResolvePolygonAsync(
        CancellationToken cancellationToken)
    {
        var zone = await _workZones.GetActiveBySiteIdAsync(SiteIds.Cxr, cancellationToken);
        var ring = zone?.GetPolygon();
        if (ring is { Count: >= 3 })
        {
            return ring;
        }

        return WorkZone.CxrDefaultPolygon();
    }
}
