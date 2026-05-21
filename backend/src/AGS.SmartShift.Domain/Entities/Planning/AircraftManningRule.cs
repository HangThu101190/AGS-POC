using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class AircraftManningRule : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string AircraftPattern { get; private set; } = string.Empty;
    public int BaseManning { get; private set; }
    public bool IsActive { get; private set; }

    private AircraftManningRule()
    {
    }

    public static AircraftManningRule Create(
        Guid siteId,
        string aircraftPattern,
        int baseManning,
        DateTime utcNow)
    {
        var rule = new AircraftManningRule
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            AircraftPattern = aircraftPattern.Trim().ToUpperInvariant(),
            BaseManning = Math.Max(1, baseManning),
            IsActive = true,
        };
        rule.MarkCreated(utcNow);
        return rule;
    }

    public void Update(string aircraftPattern, int baseManning, DateTime utcNow)
    {
        AircraftPattern = aircraftPattern.Trim().ToUpperInvariant();
        BaseManning = Math.Max(1, baseManning);
        MarkUpdated(utcNow);
    }

    public void SetActive(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        MarkUpdated(utcNow);
    }
}
