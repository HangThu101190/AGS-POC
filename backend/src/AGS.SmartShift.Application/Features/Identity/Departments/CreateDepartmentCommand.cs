using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed record CreateDepartmentCommand(DepartmentUpsertDto Body) : IRequest<DepartmentDto>;
