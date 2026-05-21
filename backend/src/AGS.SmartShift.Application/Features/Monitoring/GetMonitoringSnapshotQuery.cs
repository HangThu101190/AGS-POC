using MediatR;

namespace AGS.SmartShift.Application.Features.Monitoring;

public sealed record GetMonitoringSnapshotQuery(DateTime? SinceUtc = null)
    : IRequest<MonitoringSnapshotResult>;

public sealed class MonitoringSnapshotDto
{
    public DateTime GeneratedAtUtc { get; init; }
    public int InShiftCount { get; init; }
    public int NotCheckedInCount { get; init; }
    public int CheckedOutCount { get; init; }
    public int OutsideZoneCount { get; init; }
    public MonitoringWorkZoneDto WorkZone { get; init; } = MonitoringWorkZoneDto.CxrDefault();
    public IReadOnlyList<MonitoringMarkerDto> Markers { get; init; } = [];
    public IReadOnlyList<MonitoringWorkforceEntryDto> CheckedIn { get; init; } = [];
    public IReadOnlyList<MonitoringWorkforceEntryDto> NotCheckedIn { get; init; } = [];
    public IReadOnlyList<MonitoringWorkforceEntryDto> CheckedOut { get; init; } = [];
}

public sealed class MonitoringWorkforceEntryDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string DepartmentCode { get; init; } = string.Empty;
}

public sealed class MonitoringWorkZoneDto
{
    public string Name { get; init; } = "Khu vực làm việc CXR";
    public IReadOnlyList<MonitoringLatLngDto> Polygon { get; init; } = [];

    public static MonitoringWorkZoneDto CxrDefault() => new()
    {
        Polygon =
        [
            new() { Lat = 11.9980, Lng = 109.2188 },
            new() { Lat = 11.9980, Lng = 109.2196 },
            new() { Lat = 11.9984, Lng = 109.2196 },
            new() { Lat = 11.9984, Lng = 109.2188 },
        ],
    };
}

public sealed class MonitoringLatLngDto
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}

public sealed class MonitoringMarkerDto
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string DepartmentCode { get; init; } = string.Empty;
    public double Lat { get; init; }
    public double Lng { get; init; }
    public bool InZone { get; init; }
    public string? GeoNote { get; init; }
    public bool CheckedOut { get; init; }
    public DateTime CheckInUtc { get; init; }
    public DateTime? CheckOutUtc { get; init; }
    public IReadOnlyList<string> ShiftSegments { get; init; } = [];
    public IReadOnlyList<string> FlightNos { get; init; } = [];
    public IReadOnlyList<MonitoringGpsPointDto> History { get; init; } = [];
}

public sealed class MonitoringGpsPointDto
{
    public DateTime AtUtc { get; init; }
    public double Lat { get; init; }
    public double Lng { get; init; }
}
