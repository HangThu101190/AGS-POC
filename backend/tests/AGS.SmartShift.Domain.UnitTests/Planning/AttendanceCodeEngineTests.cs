using AGS.SmartShift.Application.Planning;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Planning;

public sealed class AttendanceCodeEngineTests
{
    [Fact]
    public void Empty_segments_returns_weekoff_T()
    {
        var r = AttendanceCodeEngine.FromSegments(Array.Empty<string>());
        Assert.Equal("T", r.Code);
        Assert.Equal("weekoff", r.Type);
    }

    [Fact]
    public void Holiday_date_returns_L()
    {
        var r = AttendanceCodeEngine.FromSegments(new[] { "07-15" }, "01/05/2026");
        Assert.Equal("L", r.Code);
        Assert.Equal("holiday", r.Type);
    }

    [Fact]
    public void Day_shift_splits_N_and_D_hours()
    {
        var r = AttendanceCodeEngine.FromSegments(new[] { "07-15" }, "10/05/2026");
        Assert.Equal("work", r.Type);
        Assert.Contains("N", r.Code);
        Assert.True(r.DayHours > 0);
    }

    [Fact]
    public void Leave_flag_overrides_segments()
    {
        var r = AttendanceCodeEngine.FromSegments(
            new[] { "07-15" },
            flags: new AttendanceCodeFlags { LeaveCode = "F" });
        Assert.Equal("F", r.Code);
        Assert.Equal("leave", r.Type);
    }
}
