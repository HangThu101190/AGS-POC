using AGS.SmartShift.Application.Common.Auth;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Me;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<AuthUserDto>>
{
    private readonly IUserAccountRepository _accounts;
    private readonly AuthUserDtoBuilder _userDtoBuilder;

    public GetCurrentUserQueryHandler(IUserAccountRepository accounts, AuthUserDtoBuilder userDtoBuilder)
    {
        _accounts = accounts;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<Result<AuthUserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetByIdWithEmployeeAsync(query.UserAccountId, cancellationToken);
        if (account?.Employee is null || !account.IsActive || !account.Employee.IsActive)
        {
            return Result<AuthUserDto>.Failure("User not found.");
        }

        var dto = await _userDtoBuilder.TryBuildAsync(account, cancellationToken);
        return dto is null
            ? Result<AuthUserDto>.Failure("User not found.")
            : Result<AuthUserDto>.Success(dto);
    }
}
