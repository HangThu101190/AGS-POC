using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _departments;
    private readonly IDateTimeProvider _clock;

    public UpdateDepartmentCommandHandler(IDepartmentRepository departments, IDateTimeProvider clock)
    {
        _departments = departments;
        _clock = clock;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departments.GetTrackedByIdAsync(request.DepartmentId, cancellationToken)
            ?? throw new DomainException("department_not_found", "Không tìm thấy phòng ban.");

        department.UpdateDetails(request.Body.Name, _clock.UtcNow);
        if (request.Body.AllowedRoles is not null)
        {
            department.SetAllowedRoles(request.Body.AllowedRoles.ToList(), _clock.UtcNow);
        }

        await _departments.SaveChangesAsync(cancellationToken);
        return IdentityDtoMapper.ToDepartmentDto(department);
    }
}
