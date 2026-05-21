using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IUserAccountRepository _accounts;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IUserAccountRepository accounts,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _employees = employees;
        _departments = departments;
        _accounts = accounts;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var body = request.Body;
        var department = await _departments.GetByIdAsync(body.DepartmentId, cancellationToken)
            ?? throw new DomainException("department_not_found", "Phòng ban không tồn tại.");

        if (await _employees.CodeExistsAsync(body.Code, cancellationToken: cancellationToken))
        {
            throw new DomainException("employee_code_exists", "Mã nhân viên đã tồn tại.");
        }

        var role = UserRoleParser.Parse(body.Role);
        DepartmentRoleGuard.EnsureRoleAllowed(department, body.Role);

        var employee = Employee.Create(
            body.DepartmentId,
            body.Code,
            body.Name,
            role,
            _clock.UtcNow,
            body.ManagerId);

        await _employees.AddAsync(employee, cancellationToken);

        var hasUser = false;
        bool? userIsActive = null;

        if (!string.IsNullOrWhiteSpace(body.InitialPassword))
        {
            if (!PasswordPolicy.TryValidate(body.InitialPassword, out var pwdError))
            {
                throw new DomainException("invalid_password", pwdError ?? "Mật khẩu không hợp lệ.");
            }

            var account = UserAccount.Create(
                employee.Id,
                employee.Code,
                _passwordHasher.Hash(body.InitialPassword),
                _clock.UtcNow,
                mustChangePassword: false);
            await _accounts.AddAsync(account, cancellationToken);
            hasUser = true;
            userIsActive = account.IsActive;
        }

        return IdentityDtoMapper.ToEmployeeDto(
            employee,
            hasUser ? new Dictionary<Guid, bool> { [employee.Id] = userIsActive ?? true } : null);
    }
}
