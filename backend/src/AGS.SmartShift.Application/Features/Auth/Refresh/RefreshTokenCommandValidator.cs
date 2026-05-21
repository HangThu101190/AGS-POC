using FluentValidation;

namespace AGS.SmartShift.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken).NotEmpty();
    }
}
