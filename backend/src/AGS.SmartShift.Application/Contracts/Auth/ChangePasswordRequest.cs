namespace AGS.SmartShift.Application.Contracts.Auth;

public sealed class ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}
