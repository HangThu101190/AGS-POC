using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly SmartShiftDbContext _db;

    public EmployeeRepository(SmartShiftDbContext db) => _db = db;

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Employee?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var q = _db.Employees.AsNoTracking().Where(e => e.Code == normalized);
        if (excludeId is Guid id)
        {
            q = q.Where(e => e.Id != id);
        }

        return q.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _db.Employees.AddAsync(employee, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, int>> CountActiveStaffByDepartmentCodeAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from e in _db.Employees.AsNoTracking()
            join d in _db.Departments.AsNoTracking() on e.DepartmentId equals d.Id
            where e.IsActive && e.Role == Domain.Enums.UserRole.Staff
            group e by d.Code into g
            select new { Code = g.Key, Count = g.Count() }
        ).ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Code, r => r.Count, StringComparer.OrdinalIgnoreCase);
    }

    public Task<PagedResult<Employee>> ListAsync(
        TableListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Employees.AsNoTracking();
        if (criteria.ScopeDepartmentId is Guid scopeDept)
        {
            q = q.Where(e => e.DepartmentId == scopeDept);
        }

        q = ApplyFilters(q, criteria.Filters);
        return q.ToPagedListAsync(criteria, ApplySort, cancellationToken);
    }

    private static IQueryable<Employee> ApplyFilters(
        IQueryable<Employee> query,
        IReadOnlyDictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
        {
            return query;
        }

        if (filters.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            var pattern = $"%{name.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(e.Name, pattern));
        }

        if (filters.TryGetValue("code", out var code) && !string.IsNullOrWhiteSpace(code))
        {
            var pattern = $"%{code.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(e.Code, pattern));
        }

        if (filters.TryGetValue("role", out var role) && !string.IsNullOrWhiteSpace(role))
        {
            var pattern = $"%{role.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(EF.Property<string>(e, "Role"), pattern));
        }

        return query;
    }

    private static IQueryable<Employee> ApplySort(
        IQueryable<Employee> query,
        string? sortBy,
        bool desc) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "name" => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "role" => desc ? query.OrderByDescending(e => e.Role) : query.OrderBy(e => e.Role),
            "code" => desc ? query.OrderByDescending(e => e.Code) : query.OrderBy(e => e.Code),
            _ => desc ? query.OrderByDescending(e => e.Code) : query.OrderBy(e => e.Code),
        };

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
