using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Planning;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class PlanningSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Flights.AnyAsync(cancellationToken))
        {
            return;
        }

        var seedTime = DateTime.UtcNow;
        var (weekId, year, todayIdx, weekDates) = WeekCalendar.BuildCurrentWeek(seedTime.AddHours(7).Date);

        if (!await db.WeeklyPlans.AnyAsync(p => p.WeekId == weekId, cancellationToken))
        {
            var plan = WeeklyPlan.Create(
                IdentitySeedData.CxrSiteId,
                weekId,
                year,
                todayIdx,
                weekDates,
                seedTime);
            db.WeeklyPlans.Add(plan);
            await db.SaveChangesAsync(cancellationToken);
        }

        await ExcelFlightSeedData.SeedDayFromWorkbookAsync(
            db,
            IdentitySeedData.CxrSiteId,
            weekId,
            weekDates,
            todayIdx,
            seedTime,
            cancellationToken);
    }
}
