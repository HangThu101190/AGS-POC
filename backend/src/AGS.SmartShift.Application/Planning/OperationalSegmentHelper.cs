using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Planning;

public static class OperationalSegmentHelper
{
    public static OperationalSegment FromFlight(Flight flight)
    {
        if (!string.IsNullOrWhiteSpace(flight.Belt))
        {
            var belt = flight.Belt.ToUpperInvariant();
            if (belt.Contains("QN", StringComparison.Ordinal) || belt.Contains("2 -", StringComparison.Ordinal))
            {
                return OperationalSegment.Qn;
            }

            if (belt.Contains("QT", StringComparison.Ordinal) || belt.Contains("1 -", StringComparison.Ordinal)
                || belt.Contains("8 -", StringComparison.Ordinal))
            {
                return OperationalSegment.Qt;
            }
        }

        var route = flight.Route.ToUpperInvariant();
        if (route.Contains("CAN", StringComparison.Ordinal)
            || route.Contains("ICN", StringComparison.Ordinal)
            || route.Contains("SVO", StringComparison.Ordinal)
            || route.Contains("NRT", StringComparison.Ordinal))
        {
            return OperationalSegment.Qt;
        }

        return OperationalSegment.Qn;
    }

    public static string NormalizeFlightNo(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}
