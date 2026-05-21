using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class AuthSeedData
{
    /// <summary>Dev-only password for all seeded demo accounts. Change before any shared environment.</summary>
    public const string DevPassword = "Ags@Dev2026";

    public static async Task SeedAsync(
        SmartShiftDbContext db,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var employees = await db.Employees.AsNoTracking().ToListAsync(cancellationToken);
        if (employees.Count == 0)
        {
            return;
        }

        var linkedEmployeeIds = await db.UserAccounts
            .Select(a => a.EmployeeId)
            .ToListAsync(cancellationToken);
        var missing = employees
            .Where(e => !linkedEmployeeIds.Contains(e.Id))
            .ToList();
        if (missing.Count == 0)
        {
            return;
        }

        var seedTime = DateTime.UtcNow;
        var hash = passwordHasher.Hash(DevPassword);
        var accounts = missing.Select(e =>
            UserAccount.Create(e.Id, e.Code, hash, seedTime)).ToList();

        db.UserAccounts.AddRange(accounts);
        await db.SaveChangesAsync(cancellationToken);
    }
}
