using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Planning;

public sealed class FlightScheduleGuardTests
{
    private static FlightSchedule LockedSchedule()
    {
        var s = FlightSchedule.Create(Guid.NewGuid(), "2026-W21", DateTime.UtcNow);
        s.PublishAndLock(DateTime.UtcNow);
        return s;
    }

    [Fact]
    public void EnsureMutableForDay_WhenLockedAndCurrentWeek_AllowsToday()
    {
        var schedule = LockedSchedule();
        schedule.EnsureMutableForDay(2, 2, "import lịch bay");
    }

    [Fact]
    public void EnsureMutableForDay_WhenLockedAndFutureWeek_AllowsAnyDay()
    {
        var schedule = LockedSchedule();
        schedule.EnsureMutableForDay(0, -1, "import lịch bay");
    }

    [Fact]
    public void EnsureMutableForDay_WhenLockedAndPastWeek_Throws()
    {
        var schedule = LockedSchedule();
        Assert.Throws<DomainException>(() => schedule.EnsureMutableForDay(0, 7, "import lịch bay"));
    }
}
