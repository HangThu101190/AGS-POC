using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public DeactivateEmployeeCommandHandler(IEmployeeRepository employees, IDateTimeProvider clock)
    {
        _employees = employees;
        _clock = clock;
    }

    public async Task<EmployeeDto> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetTrackedByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        employee.SetActive(false, _clock.UtcNow);
        await _employees.SaveChangesAsync(cancellationToken);

        return IdentityDtoMapper.ToEmployeeDto(employee);
    }
}
