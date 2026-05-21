using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AGS.SmartShift.Api.Hubs;

/// <summary>HR monitoring — clients receive <c>snapshotUpdated</c> (REST remains source of truth).</summary>
[Authorize]
public sealed class MonitoringHub : Hub
{
    public const string GroupName = "monitoring";

    public async Task Subscribe()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName).ConfigureAwait(false);
        await Clients.Caller.SendAsync("snapshotUpdated").ConfigureAwait(false);
    }

    public async Task Unsubscribe()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName).ConfigureAwait(false);
    }
}
