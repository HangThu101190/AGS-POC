using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AGS.SmartShift.Api.Hubs;

[Authorize]
public sealed class PlanningHub : Hub
{
    public static string WeekGroup(string weekId) => $"planning:week:{weekId}";

    public static string DayGroup(string weekId, int dayIdx) => $"planning:day:{weekId}:{dayIdx}";

    public async Task SubscribeWeek(string weekId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, WeekGroup(weekId)).ConfigureAwait(false);
    }

    public async Task SubscribeDay(string weekId, int dayIdx)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, DayGroup(weekId, dayIdx)).ConfigureAwait(false);
        await Groups.AddToGroupAsync(Context.ConnectionId, WeekGroup(weekId)).ConfigureAwait(false);
    }

    public async Task UnsubscribeDay(string weekId, int dayIdx)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, DayGroup(weekId, dayIdx)).ConfigureAwait(false);
    }
}
