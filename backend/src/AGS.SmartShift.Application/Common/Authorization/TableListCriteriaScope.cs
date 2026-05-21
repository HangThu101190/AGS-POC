using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Application.Common.Authorization;

public static class TableListCriteriaScope
{
    /// <summary>HR and TBĐH see all departments; Sup/Staff are limited to their own department.</summary>
    public static TableListCriteria ApplyDepartmentScope(TableListCriteria criteria, ICurrentUserService user)
    {
        if (!user.IsAuthenticated
            || user.IsInRole(SmartShiftRoles.Hr)
            || user.IsInRole(SmartShiftRoles.Tbdh))
        {
            return criteria;
        }

        if (user.DepartmentId is not Guid deptId)
        {
            return criteria;
        }

        return new TableListCriteria
        {
            Page = criteria.Page,
            PageSize = criteria.PageSize,
            SortBy = criteria.SortBy,
            SortDescending = criteria.SortDescending,
            Filters = criteria.Filters,
            ScopeDepartmentId = deptId,
        };
    }
}
