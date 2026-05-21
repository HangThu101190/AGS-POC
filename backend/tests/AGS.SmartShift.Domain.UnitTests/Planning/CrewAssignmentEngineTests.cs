using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Planning;

public sealed class CrewAssignmentEngineTests
{
    [Fact]
    public void Run_fills_qn_line_and_counts_overtime_when_allow_overtime()
    {
        var now = DateTime.UtcNow;
        var siteId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var plan = DailyStaffingPlan.Create(siteId, "2026-W20", 0, "PVHK_DI", now);
        var flight = Flight.Create(
            siteId,
            "2026-W20",
            0,
            "VN100",
            null,
            "CXR-SGN",
            "08:00",
            "09:30",
            "PVHK_DI",
            2,
            now);
        var line = DailyStaffingLine.Create(plan.Id, flight.Id, OperationalSegment.Qn, 1, 2, 2, now);
        var employee = Employee.Create(deptId, "AGS0001", "Nguyen Van A", UserRole.Staff, now);
        var template = ShiftTemplate.Create(
            siteId,
            "PVHK_DI",
            "HC-S",
            "Ca sang",
            TimeOnly.Parse("06:00"),
            TimeOnly.Parse("14:00"),
            false,
            8m,
            OperationalSegment.Qn,
            1,
            now);
        var avail = EmployeeDayAvailability.Create(
            "2026-W20",
            0,
            employee.Id,
            DayAvailabilityStatus.WeekOff,
            now,
            allowOvertime: true);

        var result = CrewAssignmentEngine.Run(new CrewAssignmentEngineInput(
            plan,
            [line],
            [flight],
            [template],
            [employee],
            [],
            [avail],
            [],
            now));

        Assert.NotEmpty(result.Proposals);
        Assert.Contains(result.Proposals, p => p.StaffingLineId == line.Id && p.EmployeeId == employee.Id);
        Assert.True(result.Summary.TotalOvertime >= 1);
    }

    [Fact]
    public void Run_reports_unfilled_when_no_active_employee()
    {
        var now = DateTime.UtcNow;
        var siteId = Guid.NewGuid();
        var plan = DailyStaffingPlan.Create(siteId, "2026-W20", 1, "PVHK_DI", now);
        var flight = Flight.Create(
            siteId,
            "2026-W20",
            1,
            "VN200",
            null,
            "SGN-NRT",
            "10:00",
            "12:00",
            "PVHK_DI",
            1,
            now);
        var line = DailyStaffingLine.Create(plan.Id, flight.Id, OperationalSegment.Qt, 1, 1, 1, now);

        var result = CrewAssignmentEngine.Run(new CrewAssignmentEngineInput(
            plan,
            [line],
            [flight],
            [],
            [],
            [],
            [],
            [],
            now));

        Assert.True(result.Summary.UnfilledSlots >= 1);
        Assert.NotEmpty(result.Warnings);
    }
}
