using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AGS.SmartShift.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    }

    public int AccessTokenLifetimeSeconds => _options.AccessTokenMinutes * 60;

    public string CreateAccessToken(UserAccount account, Employee employee)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(SmartShiftClaimTypes.EmployeeId, employee.Id.ToString()),
            new(SmartShiftClaimTypes.DepartmentId, employee.DepartmentId.ToString()),
            new(SmartShiftClaimTypes.EmployeeCode, employee.Code),
            new(SmartShiftClaimTypes.DisplayName, employee.Name),
            new(ClaimTypes.Role, AuthUserMapper.ToRoleString(employee.Role)),
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    public DateTime GetRefreshTokenExpiry(DateTime utcNow) =>
        utcNow.AddDays(_options.RefreshTokenDays);
}
