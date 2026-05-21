using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

internal static class FlightMapping
{
    public static FlightDto ToDto(Flight f) => new()
    {
        Id = f.Id,
        WeekId = f.WeekId,
        DayIdx = f.DayIdx,
        ExcelRowNo = f.ExcelRowNo,
        SortOrder = f.SortOrder,
        FlightNo = f.FlightNo,
        DepartureFlightNo = f.DepartureFlightNo,
        Route = f.Route,
        Sta = f.Sta,
        Std = f.Std,
        Eta = f.Eta,
        Etd = f.Etd,
        EtaDelayMinutes = f.EtaDelayMinutes,
        EtdDelayMinutes = f.EtdDelayMinutes,
        DelayMinutes = f.DelayMinutes,
        IsDelayed = f.IsDelayed,
        Registration = f.Registration,
        Aircraft = f.Aircraft,
        Carry = f.Carry,
        DepartmentCode = f.DepartmentCode,
        Manning = f.Manning,
        ManningExplain = f.ManningExplain,
        IsVip = f.IsVip,
        Gate = f.Gate,
        Belt = f.Belt,
        Parking = f.Parking,
        Remark = f.Remark,
    };
}
