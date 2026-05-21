using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed record CreateEmployeeUserCommand(Guid EmployeeId, CreateEmployeeUserRequest Body)
    : IRequest<EmployeeUserDto>;
