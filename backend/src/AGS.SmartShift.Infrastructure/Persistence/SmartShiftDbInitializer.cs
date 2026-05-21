using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AGS.SmartShift.Infrastructure.Persistence;

public static class SmartShiftDbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        bool seedDevelopmentData,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartShiftDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SmartShiftDbContext>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var useInMemory = string.Equals(
            configuration["UseInMemoryDatabase"],
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        await FlightSeedSanitizer.StripDevManningExplainAsync(db, cancellationToken);

        if (seedDevelopmentData)
        {
            await IdentitySeedData.SeedAsync(db, cancellationToken);
            await DepartmentRolesSeedData.SeedAsync(db, cancellationToken);
            await RolePermissionCatalogSeedData.SeedAsync(db, cancellationToken);
            await StaffingCatalogSeedData.SeedAsync(db, cancellationToken);
            await StaffingDefaultsSeedData.SeedAsync(db, cancellationToken);
            await PvhkStaffPoolSeedData.SeedAsync(db, cancellationToken);
            await WorkZoneSeedData.SeedAsync(db, cancellationToken);
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await AuthSeedData.SeedAsync(db, passwordHasher, cancellationToken);
            await PlanningSeedData.SeedAsync(db, cancellationToken);
            await DemoWorkflowSeedData.SeedAsync(db, cancellationToken);
            logger.LogInformation("Development seed data applied.");
        }
    }
}
