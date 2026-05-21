using AGS.SmartShift.Domain.Planning;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Planning;

public sealed class PastDayGuardTests
{
    [Fact]
    public void IsPastDay_WhenBeforeToday_ReturnsTrue()
    {
        Assert.True(PastDayGuard.IsPastDay(2, 3));
    }

    [Fact]
    public void EnsureMutableDay_WhenPast_ThrowsPastDayMutationException()
    {
        var ex = Assert.Throws<PastDayMutationException>(() =>
            PastDayGuard.EnsureMutableDay(1, 3, "chỉnh delay"));

        Assert.Equal("past_day_read_only", ex.Code);
    }
}
