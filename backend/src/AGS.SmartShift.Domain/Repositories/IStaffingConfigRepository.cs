using AGS.SmartShift.Domain.Entities.Identity;
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

    Task<IReadOnlyList<ShiftTemplate>> ListShiftTemplatesAsync(
        Guid siteId,
        string? departmentCode,
        CancellationToken cancellationToken = default);

    Task<ShiftTemplate?> GetShiftTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ShiftTemplate?> FindShiftTemplateByCodeAsync(
        Guid siteId,
        string departmentCode,
        string code,
        CancellationToken cancellationToken = default);

    Task AddShiftTemplateAsync(ShiftTemplate template, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeQualification>> ListQualificationsForDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeQualification>> ListQualificationsForEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task ReplaceEmployeeQualificationsAsync(
        Guid employeeId,
        IReadOnlyList<EmployeeQualification> qualifications,
        CancellationToken cancellationToken = default);

    Task<Department?> FindDepartmentByCodeAsync(
        Guid siteId,
        string departmentCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> ListActiveEmployeesByDepartmentIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
