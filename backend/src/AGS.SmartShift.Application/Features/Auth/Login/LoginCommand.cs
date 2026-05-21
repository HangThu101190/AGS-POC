using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Auth;
using MediatR;

namespace AGS.SmartShift.Application.Features.Auth.Login;

public sealed record LoginCommand(LoginRequest Request) : IRequest<Result<AuthSessionDto>>;
