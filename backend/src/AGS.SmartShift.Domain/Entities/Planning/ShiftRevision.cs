using System.Text.Json;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

/// <summary>Cờ SĐ — audit when planned ≠ actual after sync confirm.</summary>
public sealed class ShiftRevision : AuditableEntity<Guid>
{
    public Guid ShiftAssignmentId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? FlightTrigger { get; private set; }
    public string ByCode { get; private set; } = string.Empty;
    public string DeltaSegmentsJson { get; private set; } = "[]";

    private ShiftRevision()
    {
    }

    public IReadOnlyList<string> GetDeltaSegments() =>
        JsonSerializer.Deserialize<List<string>>(DeltaSegmentsJson) ?? [];

    public static ShiftRevision Create(
        Guid assignmentId,
        string reason,
        string? flightTrigger,
        string byCode,
        IReadOnlyList<string> deltaSegments,
        DateTime utcNow)
    {
        var revision = new ShiftRevision
        {
            Id = Guid.NewGuid(),
            ShiftAssignmentId = assignmentId,
            Reason = reason.Trim(),
            FlightTrigger = flightTrigger?.Trim().ToUpperInvariant(),
            ByCode = byCode.Trim(),
            DeltaSegmentsJson = JsonSerializer.Serialize(deltaSegments),
        };
        revision.MarkCreated(utcNow);
        return revision;
    }
}
