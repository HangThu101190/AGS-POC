using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IFlightRepository
{
    Task<PagedResult<Flight>> ListAsync(
        string weekId,
        int? dayIdx,
        string? departmentCode,
        TableListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Flight>> ListByWeekAsync(
        string weekId,
        CancellationToken cancellationToken = default);

    Task ReplaceDayFlightsAsync(
        string weekId,
        int dayIdx,
        IReadOnlyList<Flight> flights,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<Flight> flights, CancellationToken cancellationToken = default);

    Task AddAsync(Flight flight, CancellationToken cancellationToken = default);

    Task DeleteAsync(Flight flight, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
