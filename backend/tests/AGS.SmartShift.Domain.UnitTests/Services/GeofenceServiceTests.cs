using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Services;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Services;

public sealed class GeofenceServiceTests
{
    private static readonly IReadOnlyList<(double Lat, double Lng)> Ring = WorkZone.CxrDefaultPolygon();

    [Fact]
    public void Contains_point_inside_cxr_polygon()
    {
        Assert.True(GeofenceService.Contains(11.995, 109.2167, Ring));
    }

    [Fact]
    public void Contains_point_outside_cxr_polygon()
    {
        Assert.False(GeofenceService.Contains(11.97, 109.19, Ring));
    }

    [Fact]
    public void Contains_vertex_counts_as_inside()
    {
        var (lat, lng) = Ring[0];
        Assert.True(GeofenceService.Contains(lat, lng, Ring));
    }

    [Fact]
    public void Contains_empty_ring_returns_false()
    {
        Assert.False(GeofenceService.Contains(11.995, 109.2167, Array.Empty<(double, double)>()));
    }
}
