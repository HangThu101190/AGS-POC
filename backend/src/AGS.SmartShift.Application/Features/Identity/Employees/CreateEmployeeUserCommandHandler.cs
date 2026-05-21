using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class CreateEmployeeUserCommandHandler : IRequestHandler<CreateEmployeeUserCommand, EmployeeUserDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IUserAccountRepository _accounts;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public CreateEmployeeUserCommandHandler(
        IEmployeeRepository employees,
        IUserAccountRepository accounts,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _employees = employees;
        _accounts = accounts;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<EmployeeUserDto> Handle(CreateEmployeeUserCommand command, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");

        if (await _accounts.GetByEmployeeIdAsync(employee.Id, cancellationToken) is not null)
        {
            throw new DomainException("user_exists", "Nhân viên đã có tài khoản đăng nhập.");
        }

        var generated = string.IsNullOrEmpty(command.Body.InitialPassword);
        var password = command.Body.InitialPassword ?? GenerateTemporaryPassword();
        if (!PasswordPolicy.TryValidate(password, out var error))
        {
            throw new DomainException("invalid_password", error ?? "Mật khẩu không hợp lệ.");
        }

        var account = UserAccount.Create(
            employee.Id,
            employee.Code,
            _passwordHasher.Hash(password),
            _clock.UtcNow,
            mustChangePassword: generated);

        await _accounts.AddAsync(account, cancellationToken);

        return new EmployeeUserDto
        {
            UserId = account.Id,
            LoginName = account.LoginName,
            IsActive = account.IsActive,
            HasUser = true,
            TemporaryPassword = generated ? password : null,
        };
    }

    private static string GenerateTemporaryPassword() => $"Ags@{Guid.NewGuid():N}"[..10] + "1!aA";
}
