using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest<EmployeeDto>;
