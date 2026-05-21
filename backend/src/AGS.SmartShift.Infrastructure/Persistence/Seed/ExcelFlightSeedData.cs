using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Import;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds flights from embedded <c>reference-data/Kế_hoạch_bay.xlsx</c> (same parser as import).
/// </summary>
internal static class ExcelFlightSeedData
{
    private const string WorkbookResourceName = "Seed.Data.Ke_hoach_bay.xlsx";

    public static async Task SeedDayFromWorkbookAsync(
        SmartShiftDbContext db,
        Guid siteId,
        string weekId,
        IReadOnlyList<string> weekDates,
        int todayIdx,
        DateTime seedTime,
        CancellationToken cancellationToken = default)
    {
        await using var stream = OpenWorkbookStream();
        if (stream is null)
        {
            return;
        }

        var parser = new FlightExcelParser();
        var parsed = await parser.ParseAsync(stream, cancellationToken);
        if (parsed.Rows.Count == 0)
        {
            return;
        }

        var dayIdx = ResolveDayIdx(parsed.TargetDayLabel, weekDates, todayIdx);
        var flights = parsed.Rows.Select(row =>
            Flight.Create(
                siteId,
                weekId,
                dayIdx,
                row.FlightNo,
                row.DepartureFlightNo,
                row.Route,
                row.Sta,
                row.Std,
                row.DepartmentCode,
                row.Manning,
                seedTime,
                row.Aircraft,
                row.ManningExplain,
                row.IsVip,
                row.VipNote,
                row.Gate,
                row.Belt,
                row.Parking,
                row.Remark,
                row.Eta,
                row.Etd,
                row.Registration,
                row.Carry,
                row.ExcelRowNo,
                row.SortOrder)).ToList();

        db.Flights.AddRange(flights);

        var sourceLabel = parsed.TargetDayLabel
            ?? weekDates.ElementAtOrDefault(dayIdx);
        if (!string.IsNullOrWhiteSpace(parsed.SheetRemark) || !string.IsNullOrWhiteSpace(sourceLabel))
        {
            db.FlightScheduleDays.Add(
                FlightScheduleDay.Create(
                    siteId,
                    weekId,
                    dayIdx,
                    seedTime,
                    sourceLabel,
                    parsed.SheetRemark));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Stream? OpenWorkbookStream()
    {
        var assembly = typeof(ExcelFlightSeedData).Assembly;
        return assembly.GetManifestResourceStream(WorkbookResourceName);
    }

    private static int ResolveDayIdx(string? dayLabel, IReadOnlyList<string> weekDates, int todayIdx)
    {
        if (!string.IsNullOrWhiteSpace(dayLabel))
        {
            for (var i = 0; i < weekDates.Count; i++)
            {
                if (string.Equals(weekDates[i], dayLabel, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        return todayIdx is >= 0 and <= 6 ? todayIdx : 0;
    }
}
