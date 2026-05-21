using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IUserAccountRepository _accounts;
    private readonly IDateTimeProvider _clock;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IUserAccountRepository accounts,
        IDateTimeProvider clock)
    {
        _employees = employees;
        _departments = departments;
        _accounts = accounts;
        _clock = clock;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var body = request.Body;
        var employee = await _employees.GetTrackedByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        var department = await _departments.GetByIdAsync(body.DepartmentId, cancellationToken)
            ?? throw new DomainException("department_not_found", "Phòng ban không tồn tại.");

        if (await _employees.CodeExistsAsync(body.Code, employee.Id, cancellationToken))
        {
            throw new DomainException("employee_code_exists", "Mã nhân viên đã tồn tại.");
        }

        var role = UserRoleParser.Parse(body.Role);
        DepartmentRoleGuard.EnsureRoleAllowed(department, body.Role);
        employee.UpdateProfile(body.DepartmentId, body.Name, role, _clock.UtcNow, body.ManagerId);
        await _employees.SaveChangesAsync(cancellationToken);

        var userFlags = await _accounts.GetUserActiveByEmployeeIdsAsync(
            new[] { employee.Id },
            cancellationToken);

        return IdentityDtoMapper.ToEmployeeDto(employee, userFlags);
    }
}
