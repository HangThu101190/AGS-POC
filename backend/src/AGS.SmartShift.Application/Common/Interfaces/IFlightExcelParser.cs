using AGS.SmartShift.Application.Contracts.Planning;

namespace AGS.SmartShift.Application.Common.Interfaces;

public interface IFlightExcelParser
{
    Task<FlightExcelParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}

public sealed class FlightExcelParseResult
{
    public int? TargetDayIdx { get; init; }
    public string? TargetDayLabel { get; init; }
    public string? SheetRemark { get; init; }
    public IReadOnlyList<ParsedFlightRow> Rows { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed class ParsedFlightRow
{
    public int ExcelRowNo { get; init; }
    public int SortOrder { get; init; }
    public string FlightNo { get; init; } = string.Empty;
    public string? DepartureFlightNo { get; init; }
    public string Route { get; init; } = string.Empty;
    public string Sta { get; init; } = string.Empty;
    public string Std { get; init; } = string.Empty;
    public string? Eta { get; init; }
    public string? Etd { get; init; }
    public string DepartmentCode { get; init; } = "PVHK_DI";
    public int Manning { get; init; } = 3;
    public string? ManningExplain { get; init; }
    public string? Registration { get; init; }
    public string? Aircraft { get; init; }
    public string? Carry { get; init; }
    public bool IsVip { get; init; }
    public string? VipNote { get; init; }
    public string? Gate { get; init; }
    public string? Belt { get; init; }
    public string? Parking { get; init; }
    public string? Remark { get; init; }
}
