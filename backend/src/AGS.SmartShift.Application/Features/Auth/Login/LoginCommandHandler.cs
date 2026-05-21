using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthSessionDto>>
{
    private readonly IUserAccountRepository _accounts;
    private readonly AuthUserDtoBuilder _userDtoBuilder;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;

    public LoginCommandHandler(
        IUserAccountRepository accounts,
        AuthUserDtoBuilder userDtoBuilder,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _userDtoBuilder = userDtoBuilder;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<AuthSessionDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var loginName = command.Request.Login.Trim().ToUpperInvariant();
        var account = await _accounts.GetByLoginNameAsync(loginName, cancellationToken);
        var now = _clock.UtcNow;

        if (account?.Employee is null || !account.IsActive || !account.Employee.IsActive)
        {
            return Result<AuthSessionDto>.Failure("Invalid login or password.");
        }

        if (account.IsLockedOut(now))
        {
            return Result<AuthSessionDto>.Failure("Account is temporarily locked. Try again later.");
        }

        if (!_passwordHasher.Verify(command.Request.Password, account.PasswordHash))
        {
            account.RecordFailedLogin(now);
            await _accounts.SaveChangesAsync(cancellationToken);
            return Result<AuthSessionDto>.Failure("Invalid login or password.");
        }

        account.RecordSuccessfulLogin(now);
        return await IssueSessionAsync(account, account.Employee, cancellationToken);
    }

    private async Task<Result<AuthSessionDto>> IssueSessionAsync(
        UserAccount account,
        Domain.Entities.Identity.Employee employee,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var accessToken = _jwt.CreateAccessToken(account, employee);
        var rawRefresh = _jwt.GenerateRefreshTokenValue();
        var refreshEntity = RefreshToken.Create(
            account.Id,
            _jwt.HashRefreshToken(rawRefresh),
            _jwt.GetRefreshTokenExpiry(now),
            now);

        await _accounts.AddRefreshTokenAsync(refreshEntity, cancellationToken);
        await _accounts.SaveChangesAsync(cancellationToken);

        var userDto = await _userDtoBuilder.TryBuildAsync(account, cancellationToken);
        if (userDto is null)
        {
            return Result<AuthSessionDto>.Failure("Invalid login or password.");
        }

        return Result<AuthSessionDto>.Success(new AuthSessionDto(
            accessToken,
            rawRefresh,
            _jwt.AccessTokenLifetimeSeconds,
            userDto));
    }
}
