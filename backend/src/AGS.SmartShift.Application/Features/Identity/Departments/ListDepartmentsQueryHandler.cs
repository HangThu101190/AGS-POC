using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Identity;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Identity;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Identity.Departments;

public sealed class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, PagedList<DepartmentDto>>
{
    private readonly IDepartmentRepository _departments;
    private readonly ICurrentUserService _user;

    public ListDepartmentsQueryHandler(IDepartmentRepository departments, ICurrentUserService user)
    {
        _departments = departments;
        _user = user;
    }

    public async Task<PagedList<DepartmentDto>> Handle(
        ListDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = TableListCriteriaScope.ApplyDepartmentScope(request.Table.ToCriteria(), _user);
        var page = await _departments.ListAsync(criteria, cancellationToken);
        var items = page.Items.Select(IdentityDtoMapper.ToDepartmentDto).ToList();

        return PagedList<DepartmentDto>.From(
            items,
            criteria.Page,
            criteria.EffectivePageSize,
            page.TotalCount);
    }
}
