using AGS.SmartShift.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence;

internal static class QueryableTableExtensions
{
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        TableListCriteria criteria,
        Func<IQueryable<T>, string?, bool, IQueryable<T>> applySort,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        query = applySort(query, criteria.SortBy, criteria.SortDescending);
        var items = await query
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total);
    }
}
