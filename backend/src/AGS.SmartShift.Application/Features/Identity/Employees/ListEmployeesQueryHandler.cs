using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Employees;

public sealed class ListEmployeesQueryHandler : IRequestHandler<ListEmployeesQuery, PagedList<EmployeeDto>>
{
    private readonly IEmployeeRepository _employees;
    private readonly IUserAccountRepository _accounts;
    private readonly ICurrentUserService _user;

    public ListEmployeesQueryHandler(
        IEmployeeRepository employees,
        IUserAccountRepository accounts,
        ICurrentUserService user)
    {
        _employees = employees;
        _accounts = accounts;
        _user = user;
    }

    public async Task<PagedList<EmployeeDto>> Handle(
        ListEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = TableListCriteriaScope.ApplyDepartmentScope(request.Table.ToCriteria(), _user);
        var page = await _employees.ListAsync(criteria, cancellationToken);
        var employeeIds = page.Items.Select(e => e.Id).ToList();
        var userFlags = await _accounts.GetUserActiveByEmployeeIdsAsync(employeeIds, cancellationToken);
        var items = page.Items.Select(e => IdentityDtoMapper.ToEmployeeDto(e, userFlags)).ToList();

        return PagedList<EmployeeDto>.From(
            items,
            criteria.Page,
            criteria.EffectivePageSize,
            page.TotalCount);
    }
}
