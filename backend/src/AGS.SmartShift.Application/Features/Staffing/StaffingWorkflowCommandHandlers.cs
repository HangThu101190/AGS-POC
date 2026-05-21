using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Contracts.Staffing;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Staffing;

public sealed record AutoAssignStaffingDayCommand(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<AutoAssignStaffingResultDto>;

public sealed class AutoAssignStaffingDayCommandHandler
    : IRequestHandler<AutoAssignStaffingDayCommand, AutoAssignStaffingResultDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IStaffingConfigRepository _config;
    private readonly IFlightRepository _flights;
    private readonly IEmployeeRepository _employees;
    private readonly IStaffingSupportQueries _support;
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;

    public AutoAssignStaffingDayCommandHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IStaffingConfigRepository config,
        IFlightRepository flights,
        IEmployeeRepository employees,
        IStaffingSupportQueries support,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _staffing = staffing;
        _config = config;
        _flights = flights;
        _employees = employees;
        _support = support;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<AutoAssignStaffingResultDto> Handle(
        AutoAssignStaffingDayCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetPlanAsync(week.SiteId, week.WeekId, request.DayIdx, dept, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Chưa có kế hoạch phân công. Chạy đề xuất định biên trước.");
        if (plan.IsLocked)
        {
            throw new DomainException("plan_locked", "Ngày đã phát hành, không thể tự động phân công.");
        }

        var result = await StaffingCrewAutoAssign.TryApplyAsync(
            week.SiteId,
            week.WeekId,
            request.DayIdx,
            dept,
            plan,
            _staffing,
            _config,
            _flights,
            _support,
            _clock,
            cancellationToken)
            ?? throw new DomainException(
                "auto_assign_skipped",
                "Không thể tự động phân công: chưa có dòng định biên hoặc chưa có chuyến bay trong ngày.");

        await _staffing.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            week.WeekId, request.DayIdx, "auto_assign", plan.Status.ToString(), cancellationToken);
        return result;
    }
}

public sealed record PublishStaffingDayCommand(
    string WeekId,
    int DayIdx,
    string? DepartmentCode,
    PublishStaffingDayDto? Body)
    : IRequest<PublishStaffingResultDto>;

public sealed class PublishStaffingDayCommandHandler
    : IRequestHandler<PublishStaffingDayCommand, PublishStaffingResultDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _currentUser;
    private readonly IPlanningDayNotifier _notifier;
    private readonly IDateTimeProvider _clock;

    public PublishStaffingDayCommandHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        INotificationRepository notifications,
        ICurrentUserService currentUser,
        IPlanningDayNotifier notifier,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _staffing = staffing;
        _notifications = notifications;
        _currentUser = currentUser;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<PublishStaffingResultDto> Handle(
        PublishStaffingDayCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetPlanAsync(week.SiteId, week.WeekId, request.DayIdx, dept, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Chưa có kế hoạch phân công ngày.");
        if (plan.IsLocked)
        {
            throw new DomainException("plan_locked", "Ngày đã được phát hành.");
        }

        if (plan.Status < DailyStaffingPlanStatus.Confirmed)
        {
            throw new DomainException("plan_not_confirmed", "Cần xác nhận phân công trước khi phát hành.");
        }

        var proposals = await _staffing.ListProposalsForPlanAsync(plan.Id, cancellationToken);
        var assignments = StaffingWorkflowHelpers.CopyProposalsToAssignments(proposals, _clock.UtcNow);
        if (assignments.Count == 0)
        {
            assignments = (await _staffing.ListAssignmentsForPlanAsync(plan.Id, cancellationToken)).ToList();
        }
        else
        {
            await _staffing.ReplaceAssignmentsForPlanAsync(plan.Id, assignments, cancellationToken);
        }

        var publisherId = _currentUser.EmployeeId
            ?? throw new DomainException("unauthorized", "Không xác định được người phát hành.");
        plan.Publish(publisherId, request.Body?.LockReason, _clock.UtcNow);

        var messages = assignments
            .Select(a => a.EmployeeId)
            .Distinct()
            .Select(employeeId => NotificationMessage.Create(
                employeeId,
                "staffing_published",
                "Lịch phân công đã phát hành",
                $"Lịch phân công ngày {request.DayIdx} tuần {week.WeekId} đã được phát hành.",
                _clock.UtcNow))
            .ToList();
        if (messages.Count > 0)
        {
            await _notifications.AddRangeAsync(messages, cancellationToken);
        }

        var notified = messages.Count;

        await _staffing.SaveChangesAsync(cancellationToken);
        await _notifications.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStaffingDayUpdatedAsync(
            week.WeekId, request.DayIdx, "publish", plan.Status.ToString(), cancellationToken);

        return new PublishStaffingResultDto
        {
            Status = plan.Status,
            PublishedAt = plan.PublishedAtUtc ?? _clock.UtcNow,
            NotificationsSent = notified,
        };
    }
}

public sealed record GetStaffingBioHeaderQuery(string WeekId, int DayIdx, string? DepartmentCode)
    : IRequest<StaffingBioHeaderDto>;

public sealed class GetStaffingBioHeaderQueryHandler : IRequestHandler<GetStaffingBioHeaderQuery, StaffingBioHeaderDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public GetStaffingBioHeaderQueryHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _staffing = staffing;
        _employees = employees;
        _clock = clock;
    }

    public async Task<StaffingBioHeaderDto> Handle(
        GetStaffingBioHeaderQuery request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetOrCreatePlanAsync(
            week.SiteId, week.WeekId, request.DayIdx, dept, _clock.UtcNow, cancellationToken);
        var header = await _staffing.GetBioHeaderAsync(plan.Id, cancellationToken);
        if (header is null)
        {
            return new StaffingBioHeaderDto();
        }

        return await StaffingWorkflowHelpers.MapBioHeaderAsync(header, _employees, cancellationToken);
    }
}

public sealed record UpsertStaffingBioHeaderCommand(
    string WeekId,
    int DayIdx,
    string? DepartmentCode,
    UpsertStaffingBioHeaderDto Body)
    : IRequest<StaffingBioHeaderDto>;

public sealed class UpsertStaffingBioHeaderCommandHandler
    : IRequestHandler<UpsertStaffingBioHeaderCommand, StaffingBioHeaderDto>
{
    private readonly IPlanningWeekService _weeks;
    private readonly IDailyStaffingRepository _staffing;
    private readonly IEmployeeRepository _employees;
    private readonly IDateTimeProvider _clock;

    public UpsertStaffingBioHeaderCommandHandler(
        IPlanningWeekService weeks,
        IDailyStaffingRepository staffing,
        IEmployeeRepository employees,
        IDateTimeProvider clock)
    {
        _weeks = weeks;
        _staffing = staffing;
        _employees = employees;
        _clock = clock;
    }

    public async Task<StaffingBioHeaderDto> Handle(
        UpsertStaffingBioHeaderCommand request,
        CancellationToken cancellationToken)
    {
        var week = await _weeks.GetOrCreateWeekAsync(request.WeekId, cancellationToken);
        var dept = (request.DepartmentCode ?? "PVHK_DI").Trim().ToUpperInvariant();
        var plan = await _staffing.GetOrCreatePlanAsync(
            week.SiteId, week.WeekId, request.DayIdx, dept, _clock.UtcNow, cancellationToken);
        if (plan.IsLocked)
        {
            throw new DomainException("plan_locked", "Ngày đã phát hành, không thể sửa BIO.");
        }

        var header = await _staffing.GetOrCreateBioHeaderAsync(plan.Id, _clock.UtcNow, cancellationToken);
        header.Update(
            request.Body.MorningSupEmployeeId,
            request.Body.EveningSupEmployeeId,
            request.Body.RadioNote,
            request.Body.AssignerEmployeeId,
            _clock.UtcNow);
        await _staffing.SaveChangesAsync(cancellationToken);
        return await StaffingWorkflowHelpers.MapBioHeaderAsync(header, _employees, cancellationToken);
    }
}

internal static class StaffingWorkflowHelpers
{
    public static List<FlightCrewAssignment> CopyProposalsToAssignments(
        IReadOnlyList<StaffingCrewProposal> proposals,
        DateTime utcNow)
    {
        var sort = 0;
        return proposals
            .Select(p => FlightCrewAssignment.Create(
                p.StaffingLineId,
                p.EmployeeId,
                p.CrewRole,
                p.WorkStart,
                p.WorkEnd,
                p.IsOvertime,
                sort++,
                utcNow))
            .ToList();
    }

    public static async Task<StaffingBioHeaderDto> MapBioHeaderAsync(
        DailyStaffingBioHeader header,
        IEmployeeRepository employees,
        CancellationToken cancellationToken)
    {
        async Task<string?> Name(Guid? id)
        {
            if (id is null)
            {
                return null;
            }

            var emp = await employees.GetByIdAsync(id.Value, cancellationToken);
            return emp?.Name;
        }

        return new StaffingBioHeaderDto
        {
            MorningSupEmployeeId = header.MorningSupEmployeeId,
            MorningSupName = await Name(header.MorningSupEmployeeId),
            EveningSupEmployeeId = header.EveningSupEmployeeId,
            EveningSupName = await Name(header.EveningSupEmployeeId),
            RadioNote = header.RadioNote,
            AssignerEmployeeId = header.AssignerEmployeeId,
            AssignerName = await Name(header.AssignerEmployeeId),
        };
    }
}
