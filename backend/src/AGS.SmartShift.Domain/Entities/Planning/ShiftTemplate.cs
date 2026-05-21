using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class ShiftTemplate : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string DepartmentCode { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsOvernight { get; private set; }
    public decimal MaxHours { get; private set; }
    public OperationalSegment? Segment { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private ShiftTemplate()
    {
    }

    public static ShiftTemplate Create(
        Guid siteId,
        string departmentCode,
        string code,
        string name,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isOvernight,
        decimal maxHours,
        OperationalSegment? segment,
        int sortOrder,
        DateTime utcNow)
    {
        var template = new ShiftTemplate
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            DepartmentCode = departmentCode.Trim().ToUpperInvariant(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            IsOvernight = isOvernight,
            MaxHours = maxHours,
            Segment = segment,
            SortOrder = sortOrder,
            IsActive = true,
        };
        template.MarkCreated(utcNow);
        return template;
    }

    public void Update(
        string name,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isOvernight,
        decimal maxHours,
        OperationalSegment? segment,
        int sortOrder,
        DateTime utcNow)
    {
        Name = name.Trim();
        StartTime = startTime;
        EndTime = endTime;
        IsOvernight = isOvernight;
        MaxHours = maxHours;
        Segment = segment;
        SortOrder = sortOrder;
        MarkUpdated(utcNow);
    }

    public void SetActive(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        MarkUpdated(utcNow);
    }
}
