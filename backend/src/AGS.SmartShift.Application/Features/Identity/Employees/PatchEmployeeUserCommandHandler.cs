using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class PatchEmployeeUserCommandHandler : IRequestHandler<PatchEmployeeUserCommand, EmployeeUserDto>
{
    private readonly IUserAccountRepository _accounts;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public PatchEmployeeUserCommandHandler(
        IUserAccountRepository accounts,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _employees = employees;
        _clock = clock;
    }

    public async Task<EmployeeUserDto> Handle(PatchEmployeeUserCommand command, CancellationToken cancellationToken)
    {
        if (await _employees.GetByIdAsync(command.EmployeeId, cancellationToken) is null)
        {
            throw new DomainException("employee_not_found", "Không tìm thấy nhân viên.");
        }

        var account = await _accounts.GetTrackedByEmployeeIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new DomainException("user_not_found", "Nhân viên chưa có tài khoản đăng nhập.");

        if (command.Body.IsActive is bool active)
        {
            account.SetActive(active, _clock.UtcNow);
            await _accounts.SaveChangesAsync(cancellationToken);
        }

        return new EmployeeUserDto
        {
            UserId = account.Id,
            LoginName = account.LoginName,
            IsActive = account.IsActive,
            HasUser = true,
        };
    }
}
