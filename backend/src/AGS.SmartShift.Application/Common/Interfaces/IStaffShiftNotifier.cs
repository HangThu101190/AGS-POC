namespace AGS.SmartShift.Application.Common.Interfaces;

/// <summary>Push realtime shift updates to staff mobile clients (SignalR).</summary>
public interface IStaffShiftNotifier
{
    Task NotifyShiftChangedAsync(
        IReadOnlyList<Guid> employeeIds,
        string flightNo,
        int delayMinutes,
        CancellationToken cancellationToken = default);
}
