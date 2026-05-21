using AGS.SmartShift.Domain.Common;
using FluentValidation;

namespace AGS.SmartShift.Application.Features.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Login).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Request.Password)
            .NotEmpty()
            .MaximumLength(PasswordPolicy.MaxLength)
            .Must(p => PasswordPolicy.TryValidate(p, out _))
            .WithMessage("Password does not meet complexity requirements.");
    }
}
