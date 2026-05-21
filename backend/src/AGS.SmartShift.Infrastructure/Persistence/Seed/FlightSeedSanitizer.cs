using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>Removes dev-only <c>manning_explain</c> values left from early seed/import builds.</summary>
public static class FlightSeedSanitizer
{
    public static async Task StripDevManningExplainAsync(
        SmartShiftDbContext db,
        CancellationToken cancellationToken = default)
    {
        await db.Flights
            .Where(f =>
                f.ManningExplain != null
                && (f.ManningExplain.StartsWith("Mock seed:")
                    || f.ManningExplain.StartsWith("Excel import:")))
            .ExecuteUpdateAsync(
                s => s.SetProperty(f => f.ManningExplain, (string?)null),
                cancellationToken);
    }
}
