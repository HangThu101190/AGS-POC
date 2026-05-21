using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed class DeactivateDepartmentCommandHandler : IRequestHandler<DeactivateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _departments;
    private readonly IDateTimeProvider _clock;

    public DeactivateDepartmentCommandHandler(IDepartmentRepository departments, IDateTimeProvider clock)
    {
        _departments = departments;
        _clock = clock;
    }

    public async Task<DepartmentDto> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departments.GetTrackedByIdAsync(request.DepartmentId, cancellationToken)
            ?? throw new DomainException("department_not_found", "Không tìm thấy phòng ban.");

        department.Deactivate(_clock.UtcNow);
        await _departments.SaveChangesAsync(cancellationToken);
        return IdentityDtoMapper.ToDepartmentDto(department);
    }
}
