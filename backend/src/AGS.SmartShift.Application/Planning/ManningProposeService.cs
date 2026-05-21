using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Planning;

public static class ManningProposeService
{
    public static IReadOnlyList<DailyStaffingLine> BuildLines(
        DailyStaffingPlan plan,
        IReadOnlyList<Flight> flights,
        IReadOnlyList<AircraftManningRule> aircraftRules,
        IReadOnlyList<AirlineManningRule> airlineRules,
        DateTime utcNow)
    {
        var lines = new List<DailyStaffingLine>();
        var sort = 1;
        foreach (var flight in flights.OrderBy(f => f.SortOrder))
        {
            var segment = OperationalSegmentHelper.FromFlight(flight);
            var proposed = ProposeManning(flight, aircraftRules, airlineRules);
            lines.Add(DailyStaffingLine.Create(
                plan.Id,
                flight.Id,
                segment,
                sort++,
                proposed,
                proposed,
                utcNow));
        }

        return lines;
    }

    private static int ProposeManning(
        Flight flight,
        IReadOnlyList<AircraftManningRule> aircraftRules,
        IReadOnlyList<AirlineManningRule> airlineRules)
    {
        var baseCount = 3;
        var aircraft = (flight.Aircraft ?? string.Empty).Trim().ToUpperInvariant();
        foreach (var rule in aircraftRules)
        {
            if (!string.IsNullOrEmpty(aircraft) && aircraft.Contains(rule.AircraftPattern, StringComparison.Ordinal))
            {
                baseCount = rule.BaseManning;
                break;
            }
        }

        var prefix = flight.FlightNo.Length >= 2 ? flight.FlightNo[..2] : flight.FlightNo;
        foreach (var rule in airlineRules)
        {
            if (prefix.StartsWith(rule.AirlinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                baseCount = (int)Math.Ceiling(baseCount * rule.Multiplier);
                break;
            }
        }

        if (flight.IsVip)
        {
            baseCount = (int)Math.Ceiling(baseCount * 1.5);
        }

        return Math.Max(1, baseCount);
    }
}
