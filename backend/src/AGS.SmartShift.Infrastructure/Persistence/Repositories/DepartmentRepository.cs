using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly SmartShiftDbContext _db;

    public DepartmentRepository(SmartShiftDbContext db) => _db = db;

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Department?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(
        Guid siteId,
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var q = _db.Departments.AsNoTracking().Where(d => d.SiteId == siteId && d.Code == normalized);
        if (excludeId is Guid id)
        {
            q = q.Where(d => d.Id != id);
        }

        return q.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _db.Departments.AddAsync(department, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<PagedResult<Department>> ListAsync(
        TableListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Departments.AsNoTracking();
        if (criteria.ScopeDepartmentId is Guid scopeDept)
        {
            q = q.Where(d => d.Id == scopeDept);
        }

        q = ApplyFilters(q, criteria.Filters);
        return q.ToPagedListAsync(criteria, ApplySort, cancellationToken);
    }

    private static IQueryable<Department> ApplyFilters(
        IQueryable<Department> query,
        IReadOnlyDictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
        {
            return query;
        }

        if (filters.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(d => d.Name.Contains(name));
        }

        if (filters.TryGetValue("code", out var code) && !string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(d => d.Code.Contains(code));
        }

        return query;
    }

    private static IQueryable<Department> ApplySort(
        IQueryable<Department> query,
        string? sortBy,
        bool desc) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "name" => desc ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "code" => desc ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code),
            _ => desc ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code),
        };

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
