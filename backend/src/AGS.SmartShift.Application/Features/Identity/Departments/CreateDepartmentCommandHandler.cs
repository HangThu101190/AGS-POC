using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _departments;
    private readonly ISiteRepository _sites;
    private readonly IDateTimeProvider _clock;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departments,
        ISiteRepository sites,
        IDateTimeProvider clock)
    {
        _departments = departments;
        _sites = sites;
        _clock = clock;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var body = request.Body;
        var sites = await _sites.ListAsync(cancellationToken);
        if (sites.Count == 0)
        {
            throw new DomainException("site_not_found", "Chưa có site.");
        }

        var siteId = body.SiteId ?? sites[0].Id;
        if (await _sites.GetByIdAsync(siteId, cancellationToken) is null)
        {
            throw new DomainException("site_not_found", "Site không tồn tại.");
        }

        if (await _departments.CodeExistsAsync(siteId, body.Code, cancellationToken: cancellationToken))
        {
            throw new DomainException("department_code_exists", "Mã phòng ban đã tồn tại.");
        }

        var allowed = body.AllowedRoles?.ToList()
            ?? DepartmentRolesDefaults.ForCode(body.Code).ToList();

        var department = Department.Create(siteId, body.Code, body.Name, _clock.UtcNow, allowed);
        await _departments.AddAsync(department, cancellationToken);
        return IdentityDtoMapper.ToDepartmentDto(department);
    }
}
