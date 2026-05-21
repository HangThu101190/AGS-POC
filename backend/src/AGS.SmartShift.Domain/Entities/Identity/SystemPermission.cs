namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class SystemPermission
{
    public string Code { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private SystemPermission()
    {
    }

    public static SystemPermission Create(string code, string module, string nameVi, string nameEn) =>
        new()
        {
            Code = code.Trim().ToLowerInvariant(),
            Module = module.Trim().ToLowerInvariant(),
            NameVi = nameVi.Trim(),
            NameEn = nameEn.Trim(),
        };
}
