using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Logout;

public sealed record LogoutCommand(RefreshTokenRequest Request) : IRequest<Result>;
