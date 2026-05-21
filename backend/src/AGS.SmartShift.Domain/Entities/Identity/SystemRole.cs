namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class SystemRole
{
    public string Code { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    private SystemRole()
    {
    }

    public static SystemRole Create(string code, string nameVi, string nameEn, int sortOrder, string? description = null) =>
        new()
        {
            Code = code.Trim().ToLowerInvariant(),
            NameVi = nameVi.Trim(),
            NameEn = nameEn.Trim(),
            SortOrder = sortOrder,
            Description = description?.Trim(),
        };
}
