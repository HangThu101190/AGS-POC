using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class Site : AggregateRoot<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = "Asia/Ho_Chi_Minh";

    private Site()
    {
    }

    public static Site Create(string code, string name, string timezone, DateTime utcNow)
    {
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Timezone = timezone.Trim(),
        };
        site.MarkCreated(utcNow);
        return site;
    }
}
