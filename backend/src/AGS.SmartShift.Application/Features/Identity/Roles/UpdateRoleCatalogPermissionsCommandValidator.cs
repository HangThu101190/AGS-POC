using FluentValidation;

namespace AGS.SmartShift.Application.Features.Identity.Roles;

public sealed class UpdateRoleCatalogPermissionsCommandValidator
    : AbstractValidator<UpdateRoleCatalogPermissionsCommand>
{
    public UpdateRoleCatalogPermissionsCommandValidator()
    {
        RuleFor(x => x.Body.Roles).NotEmpty();
        RuleForEach(x => x.Body.Roles).ChildRules(role =>
        {
            role.RuleFor(kv => kv.Key).NotEmpty().MaximumLength(16);
            role.RuleFor(kv => kv.Value).NotNull();
        });
    }
}
