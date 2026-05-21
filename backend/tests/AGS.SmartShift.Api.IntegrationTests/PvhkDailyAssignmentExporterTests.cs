using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Infrastructure.Export;
using ClosedXML.Excel;
using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

public sealed class PvhkDailyAssignmentExporterTests
{
    [Fact]
    public void Export_writes_qn_rows_with_departure_flight_and_assignee_names()
    {
        var flightId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var day = new StaffingDayDto
        {
            Plan = new StaffingPlanDto
            {
                Id = Guid.NewGuid(),
                WeekId = "2026-W20",
                DayIdx = 3,
                DepartmentCode = "PVHK_DI",
                Status = DailyStaffingPlanStatus.CrewDraft,
            },
            Lines =
            [
                new StaffingLineDto
                {
                    Id = lineId,
                    FlightId = flightId,
                    Segment = OperationalSegment.Qn,
                    SortOrder = 1,
                    TargetManning = 2,
                    ProposedManning = 2,
                },
            ],
            Flights =
            [
                new StaffingFlightDto
                {
                    Id = flightId,
                    FlightNo = "VN123",
                    DepartureFlightNo = "VN123D",
                    Route = "CXR-SGN",
                    Sta = "08:00",
                    Std = "09:30",
                    Manning = 2,
                },
            ],
            Assignments =
            [
                new StaffingAssignmentDto
                {
                    Id = Guid.NewGuid(),
                    StaffingLineId = lineId,
                    EmployeeId = Guid.NewGuid(),
                    EmployeeCode = "AGS0001",
                    EmployeeName = "Nguyen Van A",
                    Role = CrewRole.Counter,
                    WorkStart = "06:00",
                    WorkEnd = "14:00",
                },
                new StaffingAssignmentDto
                {
                    Id = Guid.NewGuid(),
                    StaffingLineId = lineId,
                    EmployeeId = Guid.NewGuid(),
                    EmployeeCode = "AGS0002",
                    EmployeeName = "Tran Thi B",
                    Role = CrewRole.Gate,
                    WorkStart = "06:00",
                    WorkEnd = "14:00",
                },
            ],
        };

        var exporter = new PvhkDailyAssignmentExporter();
        var bytes = exporter.Export(new PvhkExportRequest { Day = day, DayLabel = "10/05", CalendarYear = 2026 });

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var sheet = wb.Worksheets.First();
        Assert.Contains("CÔNG TY", sheet.Cell(1, 1).GetString(), StringComparison.Ordinal);
        Assert.Contains("10/ 05 / 2026", sheet.Cell(4, 1).GetString(), StringComparison.Ordinal);
        Assert.Equal("VN123D", sheet.Cell(10, 2).GetString());
        Assert.Equal("SGN", sheet.Cell(10, 3).GetString());
        Assert.Equal("Nguyen Van A", sheet.Cell(10, 6).GetString());
        Assert.Equal("Tran Thi B", sheet.Cell(10, 7).GetString());
    }

    [Fact]
    public void ExportWeek_creates_one_sheet_per_day_from_template()
    {
        var exporter = new PvhkDailyAssignmentExporter();
        var day = BuildSampleDay();
        var bytes = exporter.ExportWeek(new PvhkWeekExportRequest
        {
            Days =
            [
                new PvhkExportRequest { Day = day, DayLabel = "18/05", CalendarYear = 2026 },
                new PvhkExportRequest { Day = day, DayLabel = "19/05", CalendarYear = 2026 },
            ],
        });

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal(2, wb.Worksheets.Count);
        Assert.Equal("18.05", wb.Worksheet(1).Name);
        Assert.Equal("19.05", wb.Worksheet(2).Name);
        Assert.Contains("CÔNG TY", wb.Worksheet(1).Cell(1, 1).GetString(), StringComparison.Ordinal);
        Assert.Contains("18/ 05 / 2026", wb.Worksheet(1).Cell(4, 1).GetString(), StringComparison.Ordinal);
        Assert.Contains("19/ 05 / 2026", wb.Worksheet(2).Cell(4, 1).GetString(), StringComparison.Ordinal);
    }

    private static StaffingDayDto BuildSampleDay()
    {
        var flightId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        return new StaffingDayDto
        {
            Plan = new StaffingPlanDto
            {
                Id = Guid.NewGuid(),
                WeekId = "2026-W21",
                DayIdx = 3,
                DepartmentCode = "PVHK_DI",
                Status = DailyStaffingPlanStatus.CrewDraft,
            },
            Lines =
            [
                new StaffingLineDto
                {
                    Id = lineId,
                    FlightId = flightId,
                    Segment = OperationalSegment.Qn,
                    SortOrder = 1,
                    TargetManning = 2,
                    ProposedManning = 2,
                },
            ],
            Flights =
            [
                new StaffingFlightDto
                {
                    Id = flightId,
                    FlightNo = "VN123",
                    DepartureFlightNo = "VN123D",
                    Route = "CXR-SGN",
                    Sta = "08:00",
                    Std = "09:30",
                    Manning = 2,
                },
            ],
            Assignments =
            [
                new StaffingAssignmentDto
                {
                    Id = Guid.NewGuid(),
                    StaffingLineId = lineId,
                    EmployeeId = Guid.NewGuid(),
                    EmployeeCode = "AGS0001",
                    EmployeeName = "Nguyen Van A",
                    Role = CrewRole.Counter,
                    WorkStart = "06:00",
                    WorkEnd = "14:00",
                },
            ],
        };
    }
}
