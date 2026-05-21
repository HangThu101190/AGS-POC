using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Planning;

public sealed class DailyStaffingSlotSyncService : IDailyStaffingSlotSyncService
{
    private readonly SmartShiftDbContext _db;

    public DailyStaffingSlotSyncService(SmartShiftDbContext db) => _db = db;

    public async Task<(int SlotsUpdated, int AssignmentsCreated)> SyncAsync(
        string weekId,
        int dayIdx,
        string departmentCode,
        IReadOnlyList<FlightCrewAssignment> crewAssignments,
        IReadOnlyDictionary<Guid, Guid> lineIdToFlightId,
        IReadOnlyDictionary<Guid, Flight> flightsById,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var weekly = await _db.WeeklyPlans
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.WeekId == weekId, cancellationToken);
        if (weekly is null)
        {
            return (0, 0);
        }

        var dept = departmentCode.Trim().ToUpperInvariant();
        var daySlots = weekly.Slots
            .Where(s => s.DayIdx == dayIdx && s.DepartmentCode == dept)
            .ToList();

        var buckets = crewAssignments
            .GroupBy(a => SegmentBucket(a.WorkStart))
            .ToDictionary(g => g.Key, g => g.ToList());

        var slotsUpdated = 0;
        var assignmentsCreated = 0;
        foreach (var (bucket, bucketAssignments) in buckets)
        {
            var segment = new[] { bucket };
            var flightNos = bucketAssignments
                .Select(a =>
                {
                    if (!lineIdToFlightId.TryGetValue(a.StaffingLineId, out var flightId)
                        || !flightsById.TryGetValue(flightId, out var flight))
                    {
                        return null;
                    }

                    return flight.DepartureFlightNo ?? flight.FlightNo;
                })
                .Where(n => n != null)
                .Select(n => n!)
                .Distinct()
                .ToList();

            var headcount = bucketAssignments.Select(a => a.EmployeeId).Distinct().Count();
            var slot = daySlots.FirstOrDefault(s =>
                string.Equals(s.BucketKey, bucket, StringComparison.Ordinal));

            if (slot is null)
            {
                slot = ShiftSlot.Create(weekly.Id, dept, dayIdx, segment, headcount, flightNos, bucket);
                await _db.ShiftSlots.AddAsync(slot, cancellationToken);
                weekly.Slots.Add(slot);
                daySlots.Add(slot);
            }
            else
            {
                slot.ApplySegments(segment);
                slotsUpdated++;
            }

            var existingAssignments = await _db.ShiftAssignments
                .Where(a => a.ShiftSlotId == slot.Id)
                .ToListAsync(cancellationToken);
            _db.ShiftAssignments.RemoveRange(existingAssignments);

            foreach (var crew in bucketAssignments)
            {
                var assignment = ShiftAssignment.Create(slot.Id, crew.EmployeeId, flightNos, utcNow);
                await _db.ShiftAssignments.AddAsync(assignment, cancellationToken);
                assignmentsCreated++;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (slotsUpdated, assignmentsCreated);
    }

    private static string SegmentBucket(TimeOnly workStart)
    {
        var hour = workStart.Hour;
        return hour switch
        {
            >= 6 and < 14 => "06-14",
            >= 14 and < 22 => "14-22",
            _ => "22-06",
        };
    }
}
