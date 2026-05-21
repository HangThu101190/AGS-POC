using AGS.SmartShift.Domain.Planning;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Planning;

public sealed class FlightSlotOverlapTests
{
    [Fact]
    public void OverlapsSlotSegments_WhenStaStdCrossesSegment_ReturnsTrue()
    {
        var ok = FlightSlotOverlap.OverlapsSlotSegments(
            sta: "07:00",
            std: "14:00",
            isDelayed: false,
            eta: null,
            etd: null,
            segments: ["07-21"]);

        Assert.True(ok);
    }

    [Fact]
    public void OverlapsSlotSegments_WhenNoIntersection_ReturnsFalse()
    {
        var ok = FlightSlotOverlap.OverlapsSlotSegments(
            sta: "22:00",
            std: "23:00",
            isDelayed: false,
            eta: null,
            etd: null,
            segments: ["07-21"]);

        Assert.False(ok);
    }

    [Fact]
    public void OverlapsSlotSegments_WhenDelayed_UsesEtaEtd()
    {
        var ok = FlightSlotOverlap.OverlapsSlotSegments(
            sta: "07:00",
            std: "14:00",
            isDelayed: true,
            eta: "08:30",
            etd: "15:30",
            segments: ["07-21"]);

        Assert.True(ok);
    }
}
