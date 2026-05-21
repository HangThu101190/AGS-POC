using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

internal static class StaffingWorkflowConfigMapping
{
    public static ShiftTemplateDto ToDto(ShiftTemplate t) =>
        new()
        {
            Id = t.Id,
            DepartmentCode = t.DepartmentCode,
            Code = t.Code,
            Name = t.Name,
            StartTime = t.StartTime.ToString("HH:mm"),
            EndTime = t.EndTime.ToString("HH:mm"),
            IsOvernight = t.IsOvernight,
            MaxHours = t.MaxHours,
            Segment = t.Segment,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive,
        };
}

public sealed record ListShiftTemplatesQuery(string? DepartmentCode)
    : IRequest<IReadOnlyList<ShiftTemplateDto>>;

public sealed class ListShiftTemplatesQueryHandler
    : IRequestHandler<ListShiftTemplatesQuery, IReadOnlyList<ShiftTemplateDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;

    public ListShiftTemplatesQueryHandler(IPlanningWeekService weeks, IStaffingConfigRepository config)
    {
        _weeks = weeks;
        _config = config;
    }

    public async Task<IReadOnlyList<ShiftTemplateDto>> Handle(
        ListShiftTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var templates = await _config.ListShiftTemplatesAsync(week.SiteId, request.DepartmentCode, cancellationToken);
        return templates.Select(StaffingWorkflowConfigMapping.ToDto).ToList();
    }
}

public sealed record UpsertShiftTemplateCommand(Guid? Id, UpsertShiftTemplateDto Body)
    : IRequest<ShiftTemplateDto>;

public sealed class UpsertShiftTemplateCommandHandler : IRequestHandler<UpsertShiftTemplateCommand, ShiftTemplateDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public UpsertShiftTemplateCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task<ShiftTemplateDto> Handle(UpsertShiftTemplateCommand request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var now = _clock.UtcNow;
        var dept = request.Body.DepartmentCode.Trim().ToUpperInvariant();
        var code = request.Body.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("invalid_code", "Mã ca không được để trống.");
        }

        var start = TimeOnly.Parse(request.Body.StartTime.Trim());
        var end = TimeOnly.Parse(request.Body.EndTime.Trim());

        ShiftTemplate template;
        if (request.Id is { } id)
        {
            template = await _config.GetShiftTemplateByIdAsync(id, cancellationToken)
                ?? throw new DomainException("template_not_found", "Không tìm thấy ca làm việc.");
            if (template.SiteId != week.SiteId)
            {
                throw new DomainException("template_not_found", "Không tìm thấy ca làm việc.");
            }

            template.Update(
                request.Body.Name,
                start,
                end,
                request.Body.IsOvernight,
                request.Body.MaxHours,
                request.Body.Segment,
                request.Body.SortOrder,
                now);
            if (!template.IsActive)
            {
                template.SetActive(true, now);
            }
        }
        else
        {
            var existing = await _config.FindShiftTemplateByCodeAsync(week.SiteId, dept, code, cancellationToken);
            if (existing is not null)
            {
                existing.Update(
                    request.Body.Name,
                    start,
                    end,
                    request.Body.IsOvernight,
                    request.Body.MaxHours,
                    request.Body.Segment,
                    request.Body.SortOrder,
                    now);
                if (!existing.IsActive)
                {
                    existing.SetActive(true, now);
                }

                template = existing;
            }
            else
            {
                template = ShiftTemplate.Create(
                    week.SiteId,
                    dept,
                    code,
                    request.Body.Name,
                    start,
                    end,
                    request.Body.IsOvernight,
                    request.Body.MaxHours,
                    request.Body.Segment,
                    request.Body.SortOrder,
                    now);
                await _config.AddShiftTemplateAsync(template, cancellationToken);
            }
        }

        await _config.SaveChangesAsync(cancellationToken);
        return StaffingWorkflowConfigMapping.ToDto(template);
    }
}

public sealed record DeleteShiftTemplateCommand(Guid Id) : IRequest;

public sealed class DeleteShiftTemplateCommandHandler : IRequestHandler<DeleteShiftTemplateCommand>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public DeleteShiftTemplateCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task Handle(DeleteShiftTemplateCommand request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var template = await _config.GetShiftTemplateByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("template_not_found", "Không tìm thấy ca làm việc.");
        if (template.SiteId != week.SiteId)
        {
            throw new DomainException("template_not_found", "Không tìm thấy ca làm việc.");
        }

        template.SetActive(false, _clock.UtcNow);
        await _config.SaveChangesAsync(cancellationToken);
    }
}

public sealed record ListEmployeeQualificationsQuery(string DepartmentCode)
    : IRequest<IReadOnlyList<EmployeeQualificationDto>>;

public sealed class ListEmployeeQualificationsQueryHandler
    : IRequestHandler<ListEmployeeQualificationsQuery, IReadOnlyList<EmployeeQualificationDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;

    public ListEmployeeQualificationsQueryHandler(IPlanningWeekService weeks, IStaffingConfigRepository config)
    {
        _weeks = weeks;
        _config = config;
    }

    public async Task<IReadOnlyList<EmployeeQualificationDto>> Handle(
        ListEmployeeQualificationsQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var deptCode = request.DepartmentCode.Trim().ToUpperInvariant();
        var department = await _config.FindDepartmentByCodeAsync(week.SiteId, deptCode, cancellationToken)
            ?? throw new DomainException("department_not_found", "Không tìm thấy phòng ban.");
        var employees = await _config.ListActiveEmployeesByDepartmentIdAsync(department.Id, cancellationToken);
        var employeeById = employees.ToDictionary(e => e.Id);
        var qualifications = await _config.ListQualificationsForDepartmentAsync(department.Id, cancellationToken);
        return qualifications
            .Where(q => employeeById.ContainsKey(q.EmployeeId))
            .Select(q =>
            {
                var emp = employeeById[q.EmployeeId];
                return new EmployeeQualificationDto
                {
                    Id = q.Id,
                    EmployeeId = q.EmployeeId,
                    EmployeeCode = emp.Code,
                    EmployeeName = emp.Name,
                    Segment = q.Segment,
                    CrewRole = q.CrewRole,
                    Proficiency = q.Proficiency,
                    IsActive = q.IsActive,
                };
            })
            .OrderBy(q => q.EmployeeCode)
            .ThenBy(q => q.Segment)
            .ThenBy(q => q.CrewRole)
            .ToList();
    }
}

public sealed record UpsertEmployeeQualificationsCommand(Guid EmployeeId, UpsertEmployeeQualificationsDto Body)
    : IRequest<IReadOnlyList<EmployeeQualificationDto>>;

public sealed class UpsertEmployeeQualificationsCommandHandler
    : IRequestHandler<UpsertEmployeeQualificationsCommand, IReadOnlyList<EmployeeQualificationDto>>
{
    private readonly IEmployeeRepository _employees;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public UpsertEmployeeQualificationsCommandHandler(
        IEmployeeRepository employees,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _employees = employees;
        _config = config;
        _clock = clock;
    }

    public async Task<IReadOnlyList<EmployeeQualificationDto>> Handle(
        UpsertEmployeeQualificationsCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");
        var now = _clock.UtcNow;
        var qualifications = request.Body.Qualifications
            .Where(e => e.IsActive)
            .Select(e => EmployeeQualification.Create(
                employee.Id,
                e.Segment,
                e.CrewRole,
                e.Proficiency,
                now))
            .ToList();
        await _config.ReplaceEmployeeQualificationsAsync(employee.Id, qualifications, cancellationToken);
        await _config.SaveChangesAsync(cancellationToken);
        return qualifications.Select(q => new EmployeeQualificationDto
        {
            Id = q.Id,
            EmployeeId = q.EmployeeId,
            EmployeeCode = employee.Code,
            EmployeeName = employee.Name,
            Segment = q.Segment,
            CrewRole = q.CrewRole,
            Proficiency = q.Proficiency,
            IsActive = q.IsActive,
        }).ToList();
    }
}
