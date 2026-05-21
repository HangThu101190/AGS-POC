using System.Security.Claims;
using AGS.SmartShift.Application.Common.Constants;
using AGS.SmartShift.Application.Common.Interfaces;

namespace AGS.SmartShift.Api.Services;

public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserAccountId => TryParseGuid(User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub"));

    public Guid? EmployeeId => TryParseGuid(User?.FindFirstValue(SmartShiftClaimTypes.EmployeeId));

    public Guid? DepartmentId => TryParseGuid(User?.FindFirstValue(SmartShiftClaimTypes.DepartmentId));

    public string? Role
    {
        get
        {
            var role = User?.FindFirstValue(ClaimTypes.Role);
            return string.IsNullOrWhiteSpace(role) ? null : role.ToLowerInvariant();
        }
    }

    public bool IsInRole(string role) =>
        User?.IsInRole(role) == true
        || string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
