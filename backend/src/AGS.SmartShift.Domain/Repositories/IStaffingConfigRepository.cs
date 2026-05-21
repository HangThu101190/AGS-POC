using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IStaffingConfigRepository
{
    Task<IReadOnlyList<AircraftManningRule>> ListAircraftRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    Task<AircraftManningRule?> GetAircraftRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AircraftManningRule?> FindAircraftRuleByPatternAsync(
        Guid siteId,
        string aircraftPattern,
        CancellationToken cancellationToken = default);

    Task AddAircraftRuleAsync(AircraftManningRule rule, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AirlineManningRule>> ListAirlineRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    Task<AirlineManningRule?> GetAirlineRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AirlineManningRule?> FindAirlineRuleByPrefixAsync(
        Guid siteId,
        string airlinePrefix,
        CancellationToken cancellationToken = default);

    Task AddAirlineRuleAsync(AirlineManningRule rule, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
