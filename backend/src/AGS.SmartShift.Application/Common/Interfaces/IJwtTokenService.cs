using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IJwtTokenService
{
    int AccessTokenLifetimeSeconds { get; }

    string CreateAccessToken(UserAccount account, Employee employee);

    string GenerateRefreshTokenValue();

    string HashRefreshToken(string rawToken);

    DateTime GetRefreshTokenExpiry(DateTime utcNow);
}
