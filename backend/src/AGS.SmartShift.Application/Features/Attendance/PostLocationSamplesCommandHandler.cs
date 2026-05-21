using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Repositories;
using AGS.SmartShift.Domain.Services;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed class PostLocationSamplesCommandHandler
    : IRequestHandler<PostLocationSamplesCommand, LocationSamplesResultDto>
{
    private const int MaxBatchSize = 60;

    private readonly IAttendanceRepository _attendance;
    private readonly IWorkZoneRepository _workZones;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public PostLocationSamplesCommandHandler(
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

    public async Task<LocationSamplesResultDto> Handle(
        PostLocationSamplesCommand request,
        CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new ForbiddenAccessException("Staff account required.");
        }

        if (request.Samples.Count == 0)
        {
            throw new DomainException("samples_required", "Cần ít nhất một điểm GPS.");
        }

        if (request.Samples.Count > MaxBatchSize)
        {
            throw new DomainException(
                "samples_too_many",
                $"Tối đa {MaxBatchSize} điểm mỗi lần gửi.");
        }

        var record = await _attendance.GetTrackedByEmployeeIdAsync(employeeId, cancellationToken);
        if (record is null || !record.IsActive)
        {
            throw new DomainException("not_checked_in", "Chưa check-in — không thể gửi vị trí.");
        }

        var polygon = await ResolvePolygonAsync(cancellationToken);
        var points = new List<AttendanceGpsPoint>();
        DateTime? lastAt = null;

        foreach (var sample in request.Samples.OrderBy(s => s.CapturedAtUtc ?? _clock.UtcNow))
        {
            if (sample.Lat < -90 || sample.Lat > 90 || sample.Lng < -180 || sample.Lng > 180)
            {
                continue;
            }

            var at = sample.CapturedAtUtc ?? _clock.UtcNow;
            var inZone = GeofenceService.Contains(sample.Lat, sample.Lng, polygon);
            points.Add(AttendanceGpsPoint.Create(Guid.NewGuid(), record.Id, at, sample.Lat, sample.Lng));
            record.ApplyGpsPosition(sample.Lat, sample.Lng, at, inZone);
            lastAt = at;
        }

        if (points.Count == 0)
        {
            throw new DomainException("samples_invalid", "Không có điểm GPS hợp lệ.");
        }

        await _attendance.AddGpsPointsAsync(points, cancellationToken);
        await _attendance.SaveChangesAsync(cancellationToken);

        return new LocationSamplesResultDto
        {
            Accepted = points.Count,
            LastCapturedAtUtc = lastAt,
        };
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
