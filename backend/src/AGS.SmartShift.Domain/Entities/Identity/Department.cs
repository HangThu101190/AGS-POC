using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class Department : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public List<string> AllowedRoles { get; private set; } = new();

    private Department()
    {
    }

    public static Department Create(
        Guid siteId,
        string code,
        string name,
        DateTime utcNow,
        IReadOnlyList<string>? allowedRoles = null)
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            AllowedRoles = NormalizeAllowedRoles(allowedRoles),
        };
        department.MarkCreated(utcNow);
        return department;
    }

    public void UpdateDetails(string name, DateTime utcNow)
    {
        Name = name.Trim();
        MarkUpdated(utcNow);
    }

    public void SetAllowedRoles(IReadOnlyList<string> allowedRoles, DateTime utcNow)
    {
        AllowedRoles = NormalizeAllowedRoles(allowedRoles);
        MarkUpdated(utcNow);
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        MarkUpdated(utcNow);
    }

    public bool AllowsRole(string roleCode) =>
        AllowedRoles.Count == 0 ||
        AllowedRoles.Contains(roleCode.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);

    private static List<string> NormalizeAllowedRoles(IReadOnlyList<string>? roles)
    {
        if (roles is null || roles.Count == 0)
        {
            return new List<string> { "hr", "sup", "staff", "tbdh" };
        }

        return roles
            .Select(r => r.Trim().ToLowerInvariant())
            .Where(r => r is "hr" or "sup" or "staff" or "tbdh")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
