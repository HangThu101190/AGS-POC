namespace AGS.SmartShift.Application.Contracts.Auth;

public sealed record AuthSessionDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    AuthUserDto User);
