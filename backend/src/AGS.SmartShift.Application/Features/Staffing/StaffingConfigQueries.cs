using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

public sealed record AircraftManningRuleDto(Guid Id, string AircraftPattern, int BaseManning, bool IsActive);

public sealed record AirlineManningRuleDto(Guid Id, string AirlinePrefix, decimal Multiplier, bool IsActive);

public sealed record ShiftCheckInPolicyDto(
    Guid Id,
    string? DepartmentCode,
    string BucketKey,
    string SegmentStart,
    string SegmentEnd,
    int CheckInEarliestMinutesBefore,
    int CheckInLatestMinutesAfterStart,
    int CheckOutEarliestMinutesBeforeEnd,
    int CheckOutLatestMinutesAfterEnd,
    bool RequireNoteWhenLate,
    bool IsActive);

public sealed record ListAircraftManningRulesQuery : IRequest<IReadOnlyList<AircraftManningRuleDto>>;

public sealed class ListAircraftManningRulesQueryHandler
    : IRequestHandler<ListAircraftManningRulesQuery, IReadOnlyList<AircraftManningRuleDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;

    public ListAircraftManningRulesQueryHandler(IPlanningWeekService weeks, IStaffingConfigRepository config)
    {
        _weeks = weeks;
        _config = config;
    }

    public async Task<IReadOnlyList<AircraftManningRuleDto>> Handle(
        ListAircraftManningRulesQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var rules = await _config.ListAircraftRulesAsync(week.SiteId, cancellationToken);
        return rules
            .Where(r => r.IsActive)
            .Select(r => new AircraftManningRuleDto(r.Id, r.AircraftPattern, r.BaseManning, r.IsActive))
            .ToList();
    }
}

public sealed record ListAirlineManningRulesQuery : IRequest<IReadOnlyList<AirlineManningRuleDto>>;

public sealed class ListAirlineManningRulesQueryHandler
    : IRequestHandler<ListAirlineManningRulesQuery, IReadOnlyList<AirlineManningRuleDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;

    public ListAirlineManningRulesQueryHandler(IPlanningWeekService weeks, IStaffingConfigRepository config)
    {
        _weeks = weeks;
        _config = config;
    }

    public async Task<IReadOnlyList<AirlineManningRuleDto>> Handle(
        ListAirlineManningRulesQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var rules = await _config.ListAirlineRulesAsync(week.SiteId, cancellationToken);
        return rules
            .Where(r => r.IsActive)
            .Select(r => new AirlineManningRuleDto(r.Id, r.AirlinePrefix, r.Multiplier, r.IsActive))
            .ToList();
    }
}

public sealed record ListShiftCheckInPoliciesQuery(string? DepartmentCode)
    : IRequest<IReadOnlyList<ShiftCheckInPolicyDto>>;

public sealed class ListShiftCheckInPoliciesQueryHandler
    : IRequestHandler<ListShiftCheckInPoliciesQuery, IReadOnlyList<ShiftCheckInPolicyDto>>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingSupportQueries _support;

    public ListShiftCheckInPoliciesQueryHandler(IPlanningWeekService weeks, IStaffingSupportQueries support)
    {
        _weeks = weeks;
        _support = support;
    }

    public async Task<IReadOnlyList<ShiftCheckInPolicyDto>> Handle(
        ListShiftCheckInPoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var policies = await _support.ListShiftCheckInPoliciesAsync(week.SiteId, dept, cancellationToken);
        return policies
            .Select(p => new ShiftCheckInPolicyDto(
                p.Id,
                p.DepartmentCode,
                p.BucketKey,
                p.SegmentStart,
                p.SegmentEnd,
                p.CheckInEarliestMinutesBefore,
                p.CheckInLatestMinutesAfterStart,
                p.CheckOutEarliestMinutesBeforeEnd,
                p.CheckOutLatestMinutesAfterEnd,
                p.RequireNoteWhenLate,
                p.IsActive))
            .ToList();
    }
}
