using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class StaffingConfigRepository : IStaffingConfigRepository
{
    private readonly SmartShiftDbContext _db;

    public StaffingConfigRepository(SmartShiftDbContext db) => _db = db;

    public Task<IReadOnlyList<AircraftManningRule>> ListAircraftRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        _db.AircraftManningRules
            .Where(r => r.SiteId == siteId)
            .OrderBy(r => r.AircraftPattern)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AircraftManningRule>)t.Result, cancellationToken);

    public Task<AircraftManningRule?> GetAircraftRuleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _db.AircraftManningRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<AircraftManningRule?> FindAircraftRuleByPatternAsync(
        Guid siteId,
        string aircraftPattern,
        CancellationToken cancellationToken = default)
    {
        var pattern = aircraftPattern.Trim().ToUpperInvariant();
        return _db.AircraftManningRules.FirstOrDefaultAsync(
            r => r.SiteId == siteId && r.AircraftPattern == pattern,
            cancellationToken);
    }

    public async Task AddAircraftRuleAsync(AircraftManningRule rule, CancellationToken cancellationToken = default) =>
        await _db.AircraftManningRules.AddAsync(rule, cancellationToken);

    public Task<IReadOnlyList<AirlineManningRule>> ListAirlineRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        _db.AirlineManningRules
            .Where(r => r.SiteId == siteId)
            .OrderBy(r => r.AirlinePrefix)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AirlineManningRule>)t.Result, cancellationToken);

    public Task<AirlineManningRule?> GetAirlineRuleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _db.AirlineManningRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<AirlineManningRule?> FindAirlineRuleByPrefixAsync(
        Guid siteId,
        string airlinePrefix,
        CancellationToken cancellationToken = default)
    {
        var prefix = airlinePrefix.Trim().ToUpperInvariant();
        return _db.AirlineManningRules.FirstOrDefaultAsync(
            r => r.SiteId == siteId && r.AirlinePrefix == prefix,
            cancellationToken);
    }

    public async Task AddAirlineRuleAsync(AirlineManningRule rule, CancellationToken cancellationToken = default) =>
        await _db.AirlineManningRules.AddAsync(rule, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
