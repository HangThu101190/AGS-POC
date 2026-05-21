using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed record UpdateEmployeeCommand(Guid EmployeeId, EmployeeUpsertDto Body) : IRequest<EmployeeDto>;
