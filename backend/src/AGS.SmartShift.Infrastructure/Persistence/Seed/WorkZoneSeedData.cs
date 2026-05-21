using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class WorkZoneSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.WorkZones.AnyAsync(cancellationToken))
        {
            return;
        }

        var seedTime = DateTime.UtcNow;
        var zone = WorkZone.Create(
            WorkZoneIds.CxrDefault,
            SiteIds.Cxr,
            "Khu vực làm việc CXR",
            1,
            WorkZone.CxrDefaultPolygon(),
            seedTime);

        db.WorkZones.Add(zone);
        await db.SaveChangesAsync(cancellationToken);
    }
}
