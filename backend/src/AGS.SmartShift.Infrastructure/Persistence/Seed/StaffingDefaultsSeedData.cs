using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class StaffingDefaultsSeedData
{
    public static async Task SeedAsync(SmartShiftDbContext db, CancellationToken cancellationToken = default)
    {
        var siteId = IdentitySeedData.CxrSiteId;
        var now = DateTime.UtcNow;
        await EnsureManningRulesAsync(db, siteId, now, cancellationToken);
        await EnsureLeaveTypesAsync(db, now, cancellationToken);
        await EnsureShiftPoliciesAsync(db, siteId, now, cancellationToken);
        await EnsureShiftTemplatesAsync(db, now, cancellationToken);
    }

    private static async Task EnsureShiftTemplatesAsync(
        SmartShiftDbContext db,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (await db.ShiftTemplates.AnyAsync(cancellationToken))
        {
            return;
        }

        db.ShiftTemplates.AddRange(ShiftTemplateSeedData.Build(now));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureManningRulesAsync(
        SmartShiftDbContext db,
        Guid siteId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (!await db.AircraftManningRules.AnyAsync(cancellationToken))
        {
            db.AircraftManningRules.AddRange(
                AircraftManningRule.Create(siteId, "320", 4, now),
                AircraftManningRule.Create(siteId, "321", 5, now),
                AircraftManningRule.Create(siteId, "350", 6, now));
        }

        if (!await db.AirlineManningRules.AnyAsync(cancellationToken))
        {
            db.AirlineManningRules.AddRange(
                AirlineManningRule.Create(siteId, "VN", 1.1m, now),
                AirlineManningRule.Create(siteId, "QH", 1m, now));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureLeaveTypesAsync(
        SmartShiftDbContext db,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (await db.LeaveTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        db.LeaveTypes.AddRange(
            LeaveType.Create("F", "Nghỉ phép", now),
            LeaveType.Create("NB", "Nghỉ bù", now),
            LeaveType.Create("O", "Ốm", now));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureShiftPoliciesAsync(
        SmartShiftDbContext db,
        Guid siteId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (await db.ShiftCheckInPolicies.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var bucket in new[] { "06-14", "14-22", "22-06" })
        {
            var (start, end) = bucket switch
            {
                "06-14" => ("06:00", "14:00"),
                "14-22" => ("14:00", "22:00"),
                _ => ("22:00", "06:00"),
            };
            db.ShiftCheckInPolicies.Add(ShiftCheckInPolicy.CreateDefault(
                siteId,
                bucket,
                start,
                end,
                now,
                "PVHK_DI"));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
