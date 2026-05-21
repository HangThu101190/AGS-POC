using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserAccountRepository _accounts;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;

    public LogoutCommandHandler(
        IUserAccountRepository accounts,
        IJwtTokenService jwt,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.RefreshToken))
        {
            return Result.Success();
        }

        var hash = _jwt.HashRefreshToken(command.Request.RefreshToken);
        var stored = await _accounts.GetRefreshTokenByHashAsync(hash, cancellationToken);
        if (stored is not null && stored.IsActive(_clock.UtcNow))
        {
            stored.Revoke(_clock.UtcNow);
            await _accounts.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
