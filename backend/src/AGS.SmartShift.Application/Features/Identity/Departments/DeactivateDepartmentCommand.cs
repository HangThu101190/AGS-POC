using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed record DeactivateDepartmentCommand(Guid DepartmentId) : IRequest<DepartmentDto>;
