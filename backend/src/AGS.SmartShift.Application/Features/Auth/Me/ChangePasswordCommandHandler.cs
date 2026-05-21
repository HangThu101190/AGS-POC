using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Me;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IUserAccountRepository _accounts;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public ChangePasswordCommandHandler(
        IUserAccountRepository accounts,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<Result<Unit>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (!PasswordPolicy.TryValidate(command.Request.NewPassword, out var policyError))
        {
            return Result<Unit>.Failure(policyError ?? "Invalid password.");
        }

        var account = await _accounts.GetTrackedByIdAsync(command.UserAccountId, cancellationToken);
        if (account?.Employee is null || !account.IsActive || !account.Employee.IsActive)
        {
            return Result<Unit>.Failure("User not found.");
        }

        if (!_passwordHasher.Verify(command.Request.CurrentPassword, account.PasswordHash))
        {
            return Result<Unit>.Failure("Current password is incorrect.");
        }

        var hash = _passwordHasher.Hash(command.Request.NewPassword);
        account.ChangePassword(hash, _clock.UtcNow, mustChangePassword: false);
        await _accounts.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
