using AGS.SmartShift.Domain.Entities.Identity;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Common;

public sealed class EntityEqualityTests
{
    [Fact]
    public void Sites_with_same_id_are_equal()
    {
        var id = Guid.NewGuid();
        var a = Site.Create("CXR", "A", "Asia/Ho_Chi_Minh", DateTime.UtcNow);
        var b = Site.Create("CXR", "B", "Asia/Ho_Chi_Minh", DateTime.UtcNow);

        typeof(Site).GetProperty(nameof(Site.Id))!.SetValue(a, id);
        typeof(Site).GetProperty(nameof(Site.Id))!.SetValue(b, id);

        Assert.Equal(a, b);
    }
}
