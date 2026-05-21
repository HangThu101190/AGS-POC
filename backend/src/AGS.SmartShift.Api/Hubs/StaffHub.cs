using AGS.SmartShift.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AGS.SmartShift.Api.Hubs;

/// <summary>Staff mobile — per-employee group; clients receive <c>shiftChanged</c>.</summary>
[Authorize]
public sealed class StaffHub : Hub
{
    public static string GroupFor(Guid employeeId) => $"staff:{employeeId:D}";

    public async Task Subscribe()
    {
        var employeeId = Context.User?.FindFirst(SmartShiftClaimTypes.EmployeeId)?.Value;
        if (employeeId is null || !Guid.TryParse(employeeId, out var id))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(id)).ConfigureAwait(false);
    }

    public async Task Unsubscribe()
    {
        var employeeId = Context.User?.FindFirst(SmartShiftClaimTypes.EmployeeId)?.Value;
        if (employeeId is null || !Guid.TryParse(employeeId, out var id))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(id)).ConfigureAwait(false);
    }
}
