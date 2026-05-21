using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Monitoring;

public sealed class GetMonitoringSnapshotQueryHandler
    : IRequestHandler<GetMonitoringSnapshotQuery, MonitoringSnapshotResult>
{
    private const int GpsHistoryLimit = 50;

    private readonly IDateTimeProvider _clock;
    private readonly IPlanningWeekService _weeks;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly IAttendanceRepository _attendance;
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IWorkZoneRepository _workZones;

    public GetMonitoringSnapshotQueryHandler(
        IDateTimeProvider clock,
        IPlanningWeekService weeks,
        IShiftAssignmentRepository assignments,
        IAttendanceRepository attendance,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IWorkZoneRepository workZones)
    {
        _clock = clock;
        _weeks = weeks;
        _assignments = assignments;
        _attendance = attendance;
        _employees = employees;
        _departments = departments;
        _workZones = workZones;
    }

    public async Task<MonitoringSnapshotResult> Handle(
        GetMonitoringSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var watermark = await _attendance.GetMonitoringWatermarkUtcAsync(cancellationToken)
            ?? _clock.UtcNow;
        if (request.SinceUtc.HasValue && watermark <= request.SinceUtc.Value)
        {
            return MonitoringSnapshotResult.UnchangedResult();
        }

        var empPage = await _employees.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 200 },
            cancellationToken);

        var activeEmployees = empPage.Items.Where(e => e.IsActive).ToList();
        var deptCodes = await LoadDeptCodesAsync(cancellationToken);
        var shiftByEmployee = await LoadTodayShiftContextAsync(cancellationToken);

        var activeRecords = await _attendance.ListActiveWithCoordinatesAsync(cancellationToken);
        var checkedOutRecords = await _attendance.ListCheckedOutAsync(cancellationToken);
        var today = _clock.UtcNow.Date;
        var checkedOutToday = checkedOutRecords
            .Where(r => r.CheckOutUtc?.Date == today && (r.CurrentLat != null || r.CheckInLat != null))
            .ToList();

        var onShiftIds = activeRecords.Select(r => r.EmployeeId).ToHashSet();
        var checkedOutTodayIds = checkedOutToday.Select(r => r.EmployeeId).ToHashSet();
        var expectedTodayIds = shiftByEmployee.Keys.ToHashSet();

        var markers = new List<MonitoringMarkerDto>();
        foreach (var att in activeRecords)
        {
            var marker = await BuildMarkerAsync(
                att,
                activeEmployees,
                deptCodes,
                shiftByEmployee,
                checkedOut: false,
                cancellationToken);
            if (marker is not null)
            {
                markers.Add(marker);
            }
        }

        foreach (var att in checkedOutToday)
        {
            if (onShiftIds.Contains(att.EmployeeId))
            {
                continue;
            }

            var marker = await BuildMarkerAsync(
                att,
                activeEmployees,
                deptCodes,
                shiftByEmployee,
                checkedOut: true,
                cancellationToken);
            if (marker is not null)
            {
                markers.Add(marker);
            }
        }

        var checkedInList = BuildWorkforce(activeEmployees, onShiftIds, deptCodes);
        var checkedOutList = BuildWorkforce(activeEmployees, checkedOutTodayIds, deptCodes);
        var notCheckedInList = activeEmployees
            .Where(e =>
                expectedTodayIds.Contains(e.Id)
                && !onShiftIds.Contains(e.Id)
                && !checkedOutTodayIds.Contains(e.Id))
            .Select(e => ToWorkforce(e, deptCodes))
            .OrderBy(e => e.EmployeeName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var workZone = await BuildWorkZoneAsync(cancellationToken);
        var outsideZoneCount = markers.Count(m => !m.CheckedOut && !m.InZone);

        return MonitoringSnapshotResult.FromSnapshot(new MonitoringSnapshotDto
        {
            GeneratedAtUtc = watermark,
            InShiftCount = activeRecords.Count,
            NotCheckedInCount = notCheckedInList.Count,
            CheckedOutCount = checkedOutList.Count,
            OutsideZoneCount = outsideZoneCount,
            WorkZone = workZone,
            Markers = markers,
            CheckedIn = checkedInList,
            NotCheckedIn = notCheckedInList,
            CheckedOut = checkedOutList,
        });
    }

    private async Task<MonitoringMarkerDto?> BuildMarkerAsync(
        Domain.Entities.Attendance.AttendanceRecord att,
        IReadOnlyList<Employee> activeEmployees,
        Dictionary<Guid, string> deptCodes,
        Dictionary<Guid, (IReadOnlyList<string> Segments, IReadOnlyList<string> Flights)> shiftByEmployee,
        bool checkedOut,
        CancellationToken cancellationToken)
    {
        var emp = activeEmployees.FirstOrDefault(e => e.Id == att.EmployeeId);
        if (emp is null)
        {
            return null;
        }

        var lat = att.CurrentLat ?? att.CheckInLat ?? 0;
        var lng = att.CurrentLng ?? att.CheckInLng ?? 0;
        var history = await _attendance.ListGpsPointsAsync(att.Id, GpsHistoryLimit, cancellationToken);
        IReadOnlyList<string> segments = [];
        IReadOnlyList<string> flights = [];
        if (shiftByEmployee.TryGetValue(emp.Id, out var shift))
        {
            segments = shift.Segments;
            flights = shift.Flights;
        }

        return new MonitoringMarkerDto
        {
            AttendanceId = att.Id,
            EmployeeId = emp.Id,
            EmployeeName = emp.Name,
            EmployeeCode = emp.Code,
            DepartmentCode = deptCodes.GetValueOrDefault(emp.DepartmentId, "—"),
            Lat = lat,
            Lng = lng,
            InZone = att.InZone,
            GeoNote = att.GeoNote,
            CheckInUtc = att.CheckInUtc,
            CheckOutUtc = att.CheckOutUtc,
            CheckedOut = checkedOut,
            ShiftSegments = segments,
            FlightNos = flights,
            History = history
                .Select(p => new MonitoringGpsPointDto
                {
                    AtUtc = p.AtUtc,
                    Lat = p.Lat,
                    Lng = p.Lng,
                })
                .ToList(),
        };
    }

    private async Task<Dictionary<Guid, (IReadOnlyList<string> Segments, IReadOnlyList<string> Flights)>>
        LoadTodayShiftContextAsync(CancellationToken cancellationToken)
    {
        var result =
            new Dictionary<Guid, (IReadOnlyList<string> Segments, IReadOnlyList<string> Flights)>();

        var plan = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var todaySlots = plan.Slots.Where(s => s.DayIdx == plan.TodayIdx).ToList();
        if (todaySlots.Count == 0)
        {
            return result;
        }

        var slotById = todaySlots.ToDictionary(s => s.Id);
        var assignments = await _assignments.ListBySlotIdsAsync(
            todaySlots.Select(s => s.Id).ToList(),
            cancellationToken);

        foreach (var assignment in assignments)
        {
            if (!slotById.TryGetValue(assignment.ShiftSlotId, out var slot))
            {
                continue;
            }

            var flights = assignment.GetFlightNos();
            if (flights.Count == 0)
            {
                flights = slot.GetFlightNos();
            }

            result[assignment.EmployeeId] = (slot.GetSegments(), flights);
        }

        return result;
    }

    private async Task<MonitoringWorkZoneDto> BuildWorkZoneAsync(CancellationToken cancellationToken)
    {
        var zone = await _workZones.GetActiveBySiteIdAsync(SiteIds.Cxr, cancellationToken);
        var ring = zone?.GetPolygon();
        if (ring is null or { Count: < 3 })
        {
            return MonitoringWorkZoneDto.CxrDefault();
        }

        return new MonitoringWorkZoneDto
        {
            Name = zone!.Name,
            Polygon = ring.Select(p => new MonitoringLatLngDto { Lat = p.Lat, Lng = p.Lng }).ToList(),
        };
    }

    private async Task<Dictionary<Guid, string>> LoadDeptCodesAsync(CancellationToken cancellationToken)
    {
        var page = await _departments.ListAsync(
            new TableListCriteria { Page = 0, PageSize = 50 },
            cancellationToken);
        return page.Items.ToDictionary(d => d.Id, d => d.Code);
    }

    private static List<MonitoringWorkforceEntryDto> BuildWorkforce(
        IReadOnlyList<Employee> employees,
        HashSet<Guid> ids,
        Dictionary<Guid, string> deptCodes) =>
        employees
            .Where(e => ids.Contains(e.Id))
            .Select(e => ToWorkforce(e, deptCodes))
            .OrderBy(e => e.EmployeeName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static MonitoringWorkforceEntryDto ToWorkforce(
        Employee emp,
        Dictionary<Guid, string> deptCodes) =>
        new()
        {
            EmployeeId = emp.Id,
            EmployeeName = emp.Name,
            EmployeeCode = emp.Code,
            DepartmentCode = deptCodes.GetValueOrDefault(emp.DepartmentId, "—"),
        };
}
