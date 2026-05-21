using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class AirlineManningRule : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string AirlinePrefix { get; private set; } = string.Empty;
    public decimal Multiplier { get; private set; }
    public bool IsActive { get; private set; }

    private AirlineManningRule()
    {
    }

    public static AirlineManningRule Create(
        Guid siteId,
        string airlinePrefix,
        decimal multiplier,
        DateTime utcNow)
    {
        var rule = new AirlineManningRule
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            AirlinePrefix = airlinePrefix.Trim().ToUpperInvariant(),
            Multiplier = multiplier <= 0 ? 1m : multiplier,
            IsActive = true,
        };
        rule.MarkCreated(utcNow);
        return rule;
    }

    public void Update(string airlinePrefix, decimal multiplier, DateTime utcNow)
    {
        AirlinePrefix = airlinePrefix.Trim().ToUpperInvariant();
        Multiplier = multiplier <= 0 ? 1m : multiplier;
        MarkUpdated(utcNow);
    }

    public void SetActive(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        MarkUpdated(utcNow);
    }
}
