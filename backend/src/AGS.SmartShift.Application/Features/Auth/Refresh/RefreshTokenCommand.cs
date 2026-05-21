using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Refresh;

public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<Result<AuthSessionDto>>;
