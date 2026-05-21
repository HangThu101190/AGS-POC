using AGS.SmartShift.Domain.Entities.Identity;
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

    public async Task<IReadOnlyList<ShiftTemplate>> ListShiftTemplatesAsync(
        Guid siteId,
        string? departmentCode,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ShiftTemplates.Where(t => t.SiteId == siteId);
        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            var dept = departmentCode.Trim().ToUpperInvariant();
            query = query.Where(t => t.DepartmentCode == dept);
        }

        return await query.OrderBy(t => t.SortOrder).ThenBy(t => t.Code).ToListAsync(cancellationToken);
    }

    public Task<ShiftTemplate?> GetShiftTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ShiftTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<ShiftTemplate?> FindShiftTemplateByCodeAsync(
        Guid siteId,
        string departmentCode,
        string code,
        CancellationToken cancellationToken = default)
    {
        var dept = departmentCode.Trim().ToUpperInvariant();
        var templateCode = code.Trim().ToUpperInvariant();
        return _db.ShiftTemplates.FirstOrDefaultAsync(
            t => t.SiteId == siteId && t.DepartmentCode == dept && t.Code == templateCode,
            cancellationToken);
    }

    public async Task AddShiftTemplateAsync(ShiftTemplate template, CancellationToken cancellationToken = default) =>
        await _db.ShiftTemplates.AddAsync(template, cancellationToken);

    public async Task<IReadOnlyList<EmployeeQualification>> ListQualificationsForDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var employeeIds = await _db.Employees
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);
        return await _db.EmployeeQualifications
            .Where(q => employeeIds.Contains(q.EmployeeId) && q.IsActive)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<EmployeeQualification>> ListQualificationsForEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        _db.EmployeeQualifications
            .Where(q => q.EmployeeId == employeeId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<EmployeeQualification>)t.Result, cancellationToken);

    public async Task ReplaceEmployeeQualificationsAsync(
        Guid employeeId,
        IReadOnlyList<EmployeeQualification> qualifications,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.EmployeeQualifications
            .Where(q => q.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);
        _db.EmployeeQualifications.RemoveRange(existing);
        if (qualifications.Count > 0)
        {
            await _db.EmployeeQualifications.AddRangeAsync(qualifications, cancellationToken);
        }
    }

    public Task<Department?> FindDepartmentByCodeAsync(
        Guid siteId,
        string departmentCode,
        CancellationToken cancellationToken = default)
    {
        var code = departmentCode.Trim().ToUpperInvariant();
        return _db.Departments.FirstOrDefaultAsync(
            d => d.SiteId == siteId && d.Code == code && d.IsActive,
            cancellationToken);
    }

    public Task<IReadOnlyList<Employee>> ListActiveEmployeesByDepartmentIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default) =>
        _db.Employees
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .OrderBy(e => e.Code)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Employee>)t.Result, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
