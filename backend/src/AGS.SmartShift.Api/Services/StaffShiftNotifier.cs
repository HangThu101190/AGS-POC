using AGS.SmartShift.Api.Hubs;
using AGS.SmartShift.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AGS.SmartShift.Api.Services;

public sealed class StaffShiftNotifier : IStaffShiftNotifier
{
    private readonly IHubContext<StaffHub> _hub;

    public StaffShiftNotifier(IHubContext<StaffHub> hub) => _hub = hub;

    public async Task NotifyShiftChangedAsync(
        IReadOnlyList<Guid> employeeIds,
        string flightNo,
        int delayMinutes,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
        {
            return;
        }

        var payload = new { flightNo, delayMinutes };
        foreach (var employeeId in employeeIds.Distinct())
        {
            await _hub.Clients
                .Group(StaffHub.GroupFor(employeeId))
                .SendAsync("shiftChanged", payload, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
