using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Me;

public sealed class UpdateMeCommandHandler : IRequestHandler<UpdateMeCommand, Result<AuthUserDto>>
{
    private readonly IUserAccountRepository _accounts;
    private readonly AuthUserDtoBuilder _userDtoBuilder;
    private readonly IDateTimeProvider _clock;

    public UpdateMeCommandHandler(
        IUserAccountRepository accounts,
        AuthUserDtoBuilder userDtoBuilder,
        IDateTimeProvider clock)
    {
        _accounts = accounts;
        _userDtoBuilder = userDtoBuilder;
        _clock = clock;
    }

    public async Task<Result<AuthUserDto>> Handle(UpdateMeCommand command, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetTrackedByIdAsync(command.UserAccountId, cancellationToken);
        if (account?.Employee is null || !account.IsActive || !account.Employee.IsActive)
        {
            return Result<AuthUserDto>.Failure("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(command.Request.PreferredLanguage))
        {
            account.UpdatePreferences(command.Request.PreferredLanguage, _clock.UtcNow);
            await _accounts.SaveChangesAsync(cancellationToken);
        }

        var dto = await _userDtoBuilder.TryBuildAsync(account, cancellationToken);
        return dto is null
            ? Result<AuthUserDto>.Failure("User not found.")
            : Result<AuthUserDto>.Success(dto);
    }
}
