using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Me;

public sealed record ChangePasswordCommand(Guid UserAccountId, ChangePasswordRequest Request)
    : IRequest<Result<Unit>>;
