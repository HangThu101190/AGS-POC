using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Features.Staffing;

internal static class StaffingMapping
{
    public static IReadOnlyDictionary<Guid, string> DepartmentCodesById(
        IReadOnlyList<Department> departments) =>
        departments.ToDictionary(d => d.Id, d => d.Code);

    public static string DepartmentCodeForEmployee(
        Employee? employee,
        IReadOnlyDictionary<Guid, string> deptCodeById)
    {
        if (employee is null || employee.DepartmentId == Guid.Empty) return string.Empty;
        return deptCodeById.TryGetValue(employee.DepartmentId, out var code) ? code : string.Empty;
    }

    public static StaffingPlanDto ToPlanDto(DailyStaffingPlan plan) =>
        new()
        {
            Id = plan.Id,
            WeekId = plan.WeekId,
            DayIdx = plan.DayIdx,
            DepartmentCode = plan.DepartmentCode,
            Status = plan.Status,
            HeaderJson = plan.HeaderJson,
            ConfirmedAt = plan.ConfirmedAtUtc,
            PublishedAt = plan.PublishedAtUtc,
            PublishedByEmployeeId = plan.PublishedByEmployeeId,
            LockReason = plan.LockReason,
            IsLocked = plan.IsLocked,
        };

    public static StaffingLineDto ToLineDto(DailyStaffingLine line) =>
        new()
        {
            Id = line.Id,
            FlightId = line.FlightId,
            Segment = line.Segment,
            SortOrder = line.SortOrder,
            TargetManning = line.TargetManning,
            ProposedManning = line.ProposedManning,
        };

    public static StaffingFlightDto ToFlightDto(Flight flight) =>
        new()
        {
            Id = flight.Id,
            FlightNo = flight.FlightNo,
            DepartureFlightNo = flight.DepartureFlightNo,
            Route = flight.Route,
            Sta = flight.Sta,
            Std = flight.Std,
            Eta = flight.Eta,
            Etd = flight.Etd,
            EtaDelayMinutes = flight.EtaDelayMinutes,
            EtdDelayMinutes = flight.EtdDelayMinutes,
            DelayMinutes = flight.DelayMinutes,
            IsDelayed = flight.IsDelayed,
            Aircraft = flight.Aircraft,
            IsVip = flight.IsVip,
            Manning = flight.Manning,
            Belt = flight.Belt,
        };
}
