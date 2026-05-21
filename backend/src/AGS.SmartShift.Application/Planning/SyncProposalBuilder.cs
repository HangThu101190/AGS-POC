using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Planning;

public static class SyncProposalBuilder
{
    public static IReadOnlyList<SyncProposalAffectedLine> BuildAffectedLines(
        Flight flight,
        IReadOnlyList<ShiftSlot> slots,
        IReadOnlyList<ShiftAssignment> assignments,
        IReadOnlyDictionary<Guid, string> employeeNames)
    {
        var flightNo = flight.FlightNo.ToUpperInvariant();
        var affectedSlots = slots
            .Where(s => s.DayIdx == flight.DayIdx)
            .Where(s => s.GetFlightNos().Any(f => string.Equals(f, flightNo, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (affectedSlots.Count == 0)
        {
            affectedSlots = slots.Where(s => s.DayIdx == flight.DayIdx).Take(3).ToList();
        }

        var slotIds = affectedSlots.Select(s => s.Id).ToHashSet();
        var bySlot = assignments.Where(a => slotIds.Contains(a.ShiftSlotId)).ToLookup(a => a.ShiftSlotId);

        var lines = new List<SyncProposalAffectedLine>();
        foreach (var slot in affectedSlots)
        {
            var current = slot.GetSegments().ToArray();
            var proposed = ProposeSegments(current, flight.DelayMinutes);
            var slotAssignments = bySlot[slot.Id].ToList();

            if (slotAssignments.Count == 0)
            {
                lines.Add(new SyncProposalAffectedLine
                {
                    SlotId = slot.Id,
                    EmployeeName = "(chưa gán NV)",
                    CurrentSegments = current,
                    ProposedSegments = proposed,
                });
                continue;
            }

            foreach (var assignment in slotAssignments)
            {
                employeeNames.TryGetValue(assignment.EmployeeId, out var name);
                lines.Add(new SyncProposalAffectedLine
                {
                    SlotId = slot.Id,
                    AssignmentId = assignment.Id,
                    EmployeeName = name ?? assignment.EmployeeId.ToString(),
                    CurrentSegments = current,
                    ProposedSegments = proposed,
                });
            }
        }

        return lines;
    }

    private static string[] ProposeSegments(string[] current, int delayMinutes)
    {
        if (delayMinutes <= 0 || current.Length == 0)
        {
            return current;
        }

        return current
            .Select(seg =>
            {
                var parts = seg.Split('-');
                if (parts.Length != 2)
                {
                    return seg;
                }

                return $"{parts[0]}-{ShiftEndLater(parts[1], delayMinutes)}";
            })
            .ToArray();
    }

    private static string ShiftEndLater(string endToken, int delayMinutes)
    {
        var token = endToken.Replace("+", "").Trim();
        if (token.Contains(':', StringComparison.Ordinal))
        {
            var parts = token.Split(':');
            if (parts.Length < 2
                || !int.TryParse(parts[0], out var h)
                || !int.TryParse(parts[1], out var m))
            {
                return endToken;
            }

            var total = h * 60 + m + delayMinutes;
            var nh = (total / 60) % 24;
            var nm = total % 60;
            return $"{nh:D2}:{nm:D2}";
        }

        if (!int.TryParse(token, out var endHour))
        {
            return endToken;
        }

        var endTotal = endHour * 60 + delayMinutes;
        var newHour = (endTotal / 60) % 24;
        return $"{newHour:D2}";
    }
}
