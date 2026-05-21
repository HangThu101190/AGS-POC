namespace AGS.SmartShift.Application.Common.Identity;

public static class DepartmentRolesDefaults
{
    private static readonly Dictionary<string, string[]> DefaultsByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HCNS"] = new[] { "hr", "tbdh" },
        ["PVHK_DI"] = new[] { "sup", "shift_leader", "staff" },
        ["PVHK_DEN"] = new[] { "staff" },
        ["RAMP"] = new[] { "staff" },
        ["BAGGAGE"] = new[] { "staff" },
    };

    public static IReadOnlyList<string> ForCode(string code) =>
        DefaultsByCode.TryGetValue(code, out var roles)
            ? roles
            : new[] { "hr", "sup", "staff", "tbdh" };
}
