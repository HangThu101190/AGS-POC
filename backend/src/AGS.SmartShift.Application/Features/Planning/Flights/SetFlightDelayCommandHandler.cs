using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Application.Common.Notifications;
using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Application.Planning;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;
using AGS.SmartShift.Domain.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class SetFlightDelayCommandHandler : IRequestHandler<SetFlightDelayCommand, FlightDto>
{
    private readonly IFlightRepository _flights;
    private readonly IWeeklyPlanRepository _plans;
    private readonly IShiftAssignmentRepository _assignments;
    private readonly ISyncProposalRepository _syncProposals;
    private readonly IEmployeeRepository _employees;
    private readonly INotificationRepository _notifications;
    private readonly IPlanningDayNotifier _planningNotifier;
    private readonly IDateTimeProvider _clock;

    public SetFlightDelayCommandHandler(
        IFlightRepository flights,
        IWeeklyPlanRepository plans,
        IShiftAssignmentRepository assignments,
        ISyncProposalRepository syncProposals,
        IEmployeeRepository employees,
        INotificationRepository notifications,
        IPlanningDayNotifier planningNotifier,
        IDateTimeProvider clock)
    {
        _flights = flights;
        _plans = plans;
        _assignments = assignments;
        _syncProposals = syncProposals;
        _employees = employees;
        _notifications = notifications;
        _planningNotifier = planningNotifier;
        _clock = clock;
    }

    public async Task<FlightDto> Handle(SetFlightDelayCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flights.GetByIdAsync(request.FlightId, cancellationToken)
            ?? throw new DomainException("flight_not_found", "Không tìm thấy chuyến bay.");

        var plan = await _plans.GetWithSlotsAsync(flight.WeekId, cancellationToken)
            ?? throw new DomainException("plan_not_found", "Không tìm thấy kế hoạch tuần.");

        PastDayGuard.EnsureMutableDay(flight.DayIdx, plan.TodayIdx, "chỉnh delay chuyến bay");

        int etaDelay;
        int etdDelay;
        if (request.DelayMinutes is int legacy
            && request.EtaDelayMinutes is null
            && request.EtdDelayMinutes is null)
        {
            etaDelay = legacy <= 0 ? 0 : legacy;
            etdDelay = etaDelay;
        }
        else
        {
            etaDelay = request.EtaDelayMinutes ?? flight.EtaDelayMinutes;
            etdDelay = request.EtdDelayMinutes ?? flight.EtdDelayMinutes;
        }

        flight.ApplyDelays(etaDelay, etdDelay, _clock.UtcNow);
        await _flights.SaveChangesAsync(cancellationToken);

        var delay = flight.DelayMinutes;
        if (delay > 0)
        {
            var existing = await _syncProposals.GetPendingByFlightIdAsync(flight.Id, cancellationToken);
            if (existing is not null)
            {
                existing.Dismiss(Guid.Empty, _clock.UtcNow);
                await _syncProposals.SaveChangesAsync(cancellationToken);
            }

            var daySlots = plan.Slots.Where(s => s.DayIdx == flight.DayIdx).ToList();
            var slotIds = daySlots.Select(s => s.Id).ToList();
            var slotAssignments = await _assignments.ListBySlotIdsAsync(slotIds, cancellationToken);
            var empIds = slotAssignments.Select(a => a.EmployeeId).Distinct().ToList();
            var employeeNames = new Dictionary<Guid, string>();
            foreach (var empId in empIds)
            {
                var emp = await _employees.GetByIdAsync(empId, cancellationToken);
                if (emp is not null)
                {
                    employeeNames[empId] = emp.Name;
                }
            }

            var affected = SyncProposalBuilder.BuildAffectedLines(
                flight,
                daySlots,
                slotAssignments,
                employeeNames);

            var proposal = ShiftSyncProposal.CreatePending(
                flight.Id,
                flight.WeekId,
                flight.DayIdx,
                flight.FlightNo,
                delay,
                affected,
                _clock.UtcNow);
            await _syncProposals.AddAsync(proposal, cancellationToken);

            await ShiftLeaderNotificationHelper.NotifyShiftLeadersAsync(
                _employees,
                _notifications,
                _clock,
                "flight_delay",
                $"Chuyến {flight.FlightNo} delay {delay} phút — có đề xuất sync ca",
                "Vui lòng xem tab Flights (mobile) hoặc đề xuất trên web Sup và xác nhận. Hệ thống không tự áp dụng.",
                cancellationToken);
        }

        await _planningNotifier.NotifyFlightsDayUpdatedAsync(
            flight.WeekId,
            flight.DayIdx,
            "delay",
            [flight.Id],
            cancellationToken);

        return FlightMapping.ToDto(flight);
    }
}
