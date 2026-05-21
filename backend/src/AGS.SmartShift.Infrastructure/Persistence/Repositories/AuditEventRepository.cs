using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Audit;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class AuditEventRepository : IAuditEventRepository
{
    private readonly SmartShiftDbContext _db;

    public AuditEventRepository(SmartShiftDbContext db) => _db = db;

    public async Task<PagedResult<AuditEventListRow>> ListAsync(
        TableListCriteria criteria,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var q =
            from e in _db.AuditEvents.AsNoTracking()
            join emp in _db.Employees.AsNoTracking()
                on e.EmployeeCode equals emp.Code into empJoin
            from emp in empJoin.DefaultIfEmpty()
            select new AuditEventListRow(e, emp != null ? emp.Name : null);

        if (fromUtc is DateTime from)
        {
            q = q.Where(x => x.Event.OccurredAtUtc >= from);
        }

        if (toUtc is DateTime to)
        {
            q = q.Where(x => x.Event.OccurredAtUtc <= to);
        }

        q = ApplyFilters(q, criteria.Filters);

        var total = await q.CountAsync(cancellationToken);
        q = ApplySort(q, criteria.SortBy, criteria.SortDescending);
        var items = await q
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditEventListRow>(items, total);
    }

    private static IQueryable<AuditEventListRow> ApplyFilters(
        IQueryable<AuditEventListRow> query,
        IReadOnlyDictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
        {
            return query;
        }

        if (filters.TryGetValue("employeeCode", out var code) && !string.IsNullOrWhiteSpace(code))
        {
            var pattern = $"%{code.Trim()}%";
            query = query.Where(x =>
                x.Event.EmployeeCode != null && EF.Functions.ILike(x.Event.EmployeeCode, pattern));
        }

        if (filters.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path))
        {
            var pattern = $"%{path.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Event.Path, pattern));
        }

        if (filters.TryGetValue("httpMethod", out var method) && !string.IsNullOrWhiteSpace(method))
        {
            var normalized = method.Trim().ToUpperInvariant();
            query = query.Where(x => x.Event.HttpMethod == normalized);
        }

        if (filters.TryGetValue("statusCode", out var statusText) &&
            int.TryParse(statusText.Trim(), out var status))
        {
            query = query.Where(x => x.Event.StatusCode == status);
        }

        return query;
    }

    private static IQueryable<AuditEventListRow> ApplySort(
        IQueryable<AuditEventListRow> query,
        string? sortBy,
        bool desc) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "employeecode" => desc
                ? query.OrderByDescending(x => x.Event.EmployeeCode)
                : query.OrderBy(x => x.Event.EmployeeCode),
            "httpmethod" => desc
                ? query.OrderByDescending(x => x.Event.HttpMethod)
                : query.OrderBy(x => x.Event.HttpMethod),
            "path" => desc
                ? query.OrderByDescending(x => x.Event.Path)
                : query.OrderBy(x => x.Event.Path),
            "statuscode" => desc
                ? query.OrderByDescending(x => x.Event.StatusCode)
                : query.OrderBy(x => x.Event.StatusCode),
            "occurredatutc" or "occurredat" => desc
                ? query.OrderByDescending(x => x.Event.OccurredAtUtc)
                : query.OrderBy(x => x.Event.OccurredAtUtc),
            _ => desc
                ? query.OrderByDescending(x => x.Event.OccurredAtUtc)
                : query.OrderBy(x => x.Event.OccurredAtUtc),
        };
}
