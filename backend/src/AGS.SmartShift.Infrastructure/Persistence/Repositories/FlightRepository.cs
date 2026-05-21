using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class FlightRepository : IFlightRepository
{
    private readonly SmartShiftDbContext _db;

    public FlightRepository(SmartShiftDbContext db) => _db = db;

    public Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Flights.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<PagedResult<Flight>> ListAsync(
        string weekId,
        int? dayIdx,
        string? departmentCode,
        TableListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Flights.AsNoTracking().Where(f => f.WeekId == weekId);
        if (dayIdx is int d)
        {
            q = q.Where(f => f.DayIdx == d);
        }

        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            var code = departmentCode.Trim().ToUpperInvariant();
            q = q.Where(f => f.DepartmentCode == code);
        }

        if (criteria.Filters != null
            && criteria.Filters.TryGetValue("search", out var search)
            && !string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            q = q.Where(f =>
                EF.Functions.ILike(f.FlightNo, pattern)
                || (f.DepartureFlightNo != null && EF.Functions.ILike(f.DepartureFlightNo, pattern))
                || EF.Functions.ILike(f.Route, pattern));
        }

        q = (criteria.SortBy?.ToLowerInvariant()) switch
        {
            "sta" => criteria.SortDescending
                ? q.OrderByDescending(f => f.Sta)
                : q.OrderBy(f => f.Sta),
            "flightno" => criteria.SortDescending
                ? q.OrderByDescending(f => f.FlightNo)
                : q.OrderBy(f => f.FlightNo),
            "sortorder" or "no" => criteria.SortDescending
                ? q.OrderByDescending(f => f.SortOrder)
                : q.OrderBy(f => f.SortOrder),
            _ => criteria.SortDescending
                ? q.OrderByDescending(f => f.DayIdx).ThenByDescending(f => f.SortOrder)
                : q.OrderBy(f => f.DayIdx).ThenBy(f => f.SortOrder),
        };

        return await q.ToPagedListAsync(criteria, static (query, _, _) => query, cancellationToken);
    }

    public Task<List<Flight>> ListByWeekAsync(string weekId, CancellationToken cancellationToken = default) =>
        _db.Flights.AsNoTracking().Where(f => f.WeekId == weekId).ToListAsync(cancellationToken);

    public async Task ReplaceDayFlightsAsync(
        string weekId,
        int dayIdx,
        IReadOnlyList<Flight> flights,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Flights
            .Where(f => f.WeekId == weekId && f.DayIdx == dayIdx)
            .ToListAsync(cancellationToken);
        _db.Flights.RemoveRange(existing);
        if (flights.Count > 0)
        {
            await _db.Flights.AddRangeAsync(flights, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyList<Flight> flights, CancellationToken cancellationToken = default)
    {
        await _db.Flights.AddRangeAsync(flights, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        await _db.Flights.AddAsync(flight, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        _db.Flights.Remove(flight);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
