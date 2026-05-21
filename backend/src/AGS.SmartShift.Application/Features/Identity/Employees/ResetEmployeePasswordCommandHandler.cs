using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class ResetEmployeePasswordCommandHandler
    : IRequestHandler<ResetEmployeePasswordCommand, ResetPasswordResultDto>
{
    private readonly IUserAccountRepository _accounts;
    private readonly IEmployeeRepository _employees;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public ResetEmployeePasswordCommandHandler(
        IUserAccountRepository accounts,
        IEmployeeRepository employees,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _employees = employees;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<ResetPasswordResultDto> Handle(
        ResetEmployeePasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (await _employees.GetByIdAsync(command.EmployeeId, cancellationToken) is null)
        {
            throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");
        }

        var account = await _accounts.GetTrackedByEmployeeIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new DomainException("user_not_found", "Nhân viên chưa có tài khoản đăng nhập.");

        var temp = $"Ags@{Guid.NewGuid():N}"[..12] + "1!aA";
        account.ChangePassword(_passwordHasher.Hash(temp), _clock.UtcNow, mustChangePassword: true);
        await _accounts.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResultDto { TemporaryPassword = temp };
    }
}
