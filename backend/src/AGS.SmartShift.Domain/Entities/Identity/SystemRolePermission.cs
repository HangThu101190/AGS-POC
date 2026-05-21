namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class SystemRolePermission
{
    public string RoleCode { get; private set; } = string.Empty;
    public string PermissionCode { get; private set; } = string.Empty;

    private SystemRolePermission()
    {
    }

    public static SystemRolePermission Create(string roleCode, string permissionCode) =>
        new()
        {
            RoleCode = roleCode.Trim().ToLowerInvariant(),
            PermissionCode = permissionCode.Trim().ToLowerInvariant(),
        };
}
