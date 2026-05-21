using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Leave;

public sealed class LeaveType : AuditableEntity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private LeaveType()
    {
    }

    public static LeaveType Create(string code, string name, DateTime utcNow)
    {
        var type = new LeaveType
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            IsActive = true,
        };
        type.MarkCreated(utcNow);
        return type;
    }
}
