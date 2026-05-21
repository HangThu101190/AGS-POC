using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthSessionDto>>
{
    private readonly IUserAccountRepository _accounts;
    private readonly AuthUserDtoBuilder _userDtoBuilder;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;

    public RefreshTokenCommandHandler(
        IUserAccountRepository accounts,
        AuthUserDtoBuilder userDtoBuilder,
        IJwtTokenService jwt,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _userDtoBuilder = userDtoBuilder;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<AuthSessionDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var hash = _jwt.HashRefreshToken(command.Request.RefreshToken);
        var stored = await _accounts.GetRefreshTokenByHashAsync(hash, cancellationToken);
        var now = _clock.UtcNow;

        if (stored is null || !stored.IsActive(now))
        {
            return Result<AuthSessionDto>.Failure("Invalid or expired refresh token.");
        }

        var account = await _accounts.GetByIdWithEmployeeAsync(stored.UserAccountId, cancellationToken);
        if (account?.Employee is null || !account.IsActive || !account.Employee.IsActive)
        {
            stored.Revoke(now);
            await _accounts.SaveChangesAsync(cancellationToken);
            return Result<AuthSessionDto>.Failure("Invalid or expired refresh token.");
        }

        stored.Revoke(now);

        var accessToken = _jwt.CreateAccessToken(account, account.Employee);
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
            return Result<AuthSessionDto>.Failure("Invalid or expired refresh token.");
        }

        return Result<AuthSessionDto>.Success(new AuthSessionDto(
            accessToken,
            rawRefresh,
            _jwt.AccessTokenLifetimeSeconds,
            userDto));
    }
}
