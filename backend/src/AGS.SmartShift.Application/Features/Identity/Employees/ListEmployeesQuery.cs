using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Common;
using AGS.SmartShift.Application.Contracts.Identity;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed record ListEmployeesQuery(TableListRequest Table) : IRequest<PagedList<EmployeeDto>>;
