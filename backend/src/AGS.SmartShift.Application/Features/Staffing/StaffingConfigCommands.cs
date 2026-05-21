using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

public sealed record UpsertAircraftManningRuleCommand(Guid? Id, UpsertAircraftManningRuleDto Body)
    : IRequest<AircraftManningRuleDto>;

public sealed class UpsertAircraftManningRuleCommandHandler
    : IRequestHandler<UpsertAircraftManningRuleCommand, AircraftManningRuleDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public UpsertAircraftManningRuleCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task<AircraftManningRuleDto> Handle(
        UpsertAircraftManningRuleCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var now = _clock.UtcNow;
        var pattern = request.Body.AircraftPattern.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new DomainException("invalid_pattern", "Loại tàu không được để trống.");
        }

        AircraftManningRule rule;
        if (request.Id is { } id)
        {
            rule = await _config.GetAircraftRuleByIdAsync(id, cancellationToken)
                ?? throw new DomainException("rule_not_found", "Không tìm thấy quy tắc định biên.");
            if (rule.SiteId != week.SiteId)
            {
                throw new DomainException("rule_not_found", "Không tìm thấy quy tắc định biên.");
            }

            var duplicate = await _config.FindAircraftRuleByPatternAsync(week.SiteId, pattern, cancellationToken);
            if (duplicate != null && duplicate.Id != rule.Id)
            {
                throw new DomainException("rule_duplicate", "Loại tàu đã tồn tại.");
            }

            rule.Update(pattern, request.Body.BaseManning, now);
            if (!rule.IsActive)
            {
                rule.SetActive(true, now);
            }
        }
        else
        {
            var existing = await _config.FindAircraftRuleByPatternAsync(week.SiteId, pattern, cancellationToken);
            if (existing != null)
            {
                existing.Update(pattern, request.Body.BaseManning, now);
                if (!existing.IsActive)
                {
                    existing.SetActive(true, now);
                }

                rule = existing;
            }
            else
            {
                rule = AircraftManningRule.Create(week.SiteId, pattern, request.Body.BaseManning, now);
                await _config.AddAircraftRuleAsync(rule, cancellationToken);
            }
        }

        await _config.SaveChangesAsync(cancellationToken);
        return new AircraftManningRuleDto(rule.Id, rule.AircraftPattern, rule.BaseManning, rule.IsActive);
    }
}

public sealed record DeleteAircraftManningRuleCommand(Guid Id) : IRequest;

public sealed class DeleteAircraftManningRuleCommandHandler : IRequestHandler<DeleteAircraftManningRuleCommand>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public DeleteAircraftManningRuleCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task Handle(DeleteAircraftManningRuleCommand request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var rule = await _config.GetAircraftRuleByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("rule_not_found", "Không tìm thấy quy tắc định biên.");
        if (rule.SiteId != week.SiteId)
        {
            throw new DomainException("rule_not_found", "Không tìm thấy quy tắc định biên.");
        }

        rule.SetActive(false, _clock.UtcNow);
        await _config.SaveChangesAsync(cancellationToken);
    }
}

public sealed record UpsertAirlineManningRuleCommand(Guid? Id, UpsertAirlineManningRuleDto Body)
    : IRequest<AirlineManningRuleDto>;

public sealed class UpsertAirlineManningRuleCommandHandler
    : IRequestHandler<UpsertAirlineManningRuleCommand, AirlineManningRuleDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public UpsertAirlineManningRuleCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task<AirlineManningRuleDto> Handle(
        UpsertAirlineManningRuleCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var now = _clock.UtcNow;
        var prefix = request.Body.AirlinePrefix.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new DomainException("invalid_prefix", "Hãng bay không được để trống.");
        }

        AirlineManningRule rule;
        if (request.Id is { } id)
        {
            rule = await _config.GetAirlineRuleByIdAsync(id, cancellationToken)
                ?? throw new DomainException("rule_not_found", "Không tìm thấy quy tắc hãng.");
            if (rule.SiteId != week.SiteId)
            {
                throw new DomainException("rule_not_found", "Không tìm thấy quy tắc hãng.");
            }

            var duplicate = await _config.FindAirlineRuleByPrefixAsync(week.SiteId, prefix, cancellationToken);
            if (duplicate != null && duplicate.Id != rule.Id)
            {
                throw new DomainException("rule_duplicate", "Hãng bay đã tồn tại.");
            }

            rule.Update(prefix, request.Body.Multiplier, now);
            if (!rule.IsActive)
            {
                rule.SetActive(true, now);
            }
        }
        else
        {
            var existing = await _config.FindAirlineRuleByPrefixAsync(week.SiteId, prefix, cancellationToken);
            if (existing != null)
            {
                existing.Update(prefix, request.Body.Multiplier, now);
                if (!existing.IsActive)
                {
                    existing.SetActive(true, now);
                }

                rule = existing;
            }
            else
            {
                rule = AirlineManningRule.Create(week.SiteId, prefix, request.Body.Multiplier, now);
                await _config.AddAirlineRuleAsync(rule, cancellationToken);
            }
        }

        await _config.SaveChangesAsync(cancellationToken);
        return new AirlineManningRuleDto(rule.Id, rule.AirlinePrefix, rule.Multiplier, rule.IsActive);
    }
}

public sealed record DeleteAirlineManningRuleCommand(Guid Id) : IRequest;

public sealed class DeleteAirlineManningRuleCommandHandler : IRequestHandler<DeleteAirlineManningRuleCommand>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IStaffingConfigRepository _config;
    private readonly IDateTimeProvider _clock;

    public DeleteAirlineManningRuleCommandHandler(
        IPlanningWeekService weeks,
        IStaffingConfigRepository config,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _config = config;
        _clock = clock;
    }

    public async Task Handle(DeleteAirlineManningRuleCommand request, CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(null, cancellationToken);
        var rule = await _config.GetAirlineRuleByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("rule_not_found", "Không tìm thấy quy tắc hãng.");
        if (rule.SiteId != week.SiteId)
        {
            throw new DomainException("rule_not_found", "Không tìm thấy quy tắc hãng.");
        }

        rule.SetActive(false, _clock.UtcNow);
        await _config.SaveChangesAsync(cancellationToken);
    }
}
