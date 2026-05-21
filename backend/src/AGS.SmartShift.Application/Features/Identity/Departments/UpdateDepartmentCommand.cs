using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed record UpdateDepartmentCommand(Guid DepartmentId, DepartmentUpsertDto Body) : IRequest<DepartmentDto>;
