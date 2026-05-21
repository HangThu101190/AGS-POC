using System.Text.Json;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Planning;

public sealed class ShiftAssignment : AuditableEntity<Guid>
{
    public Guid ShiftSlotId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string FlightNosJson { get; private set; } = "[]";
    public string Status { get; private set; } = "assigned";

    private ShiftAssignment()
    {
    }

    public static ShiftAssignment Create(
        Guid shiftSlotId,
        Guid employeeId,
        IReadOnlyList<string> flightNos,
        DateTime utcNow)
    {
        var assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftSlotId = shiftSlotId,
            EmployeeId = employeeId,
            FlightNosJson = JsonSerializer.Serialize(
                flightNos.Select(f => f.Trim().ToUpperInvariant()).Distinct().ToList()),
            Status = "assigned",
        };
        assignment.MarkCreated(utcNow);
        return assignment;
    }

    public IReadOnlyList<string> GetFlightNos() =>
        JsonSerializer.Deserialize<List<string>>(FlightNosJson) ?? [];

    public void LinkFlights(IReadOnlyList<string> flightNos, DateTime utcNow)
    {
        FlightNosJson = JsonSerializer.Serialize(
            flightNos.Select(f => f.Trim().ToUpperInvariant()).Distinct().ToList());
        MarkUpdated(utcNow);
    }
}
