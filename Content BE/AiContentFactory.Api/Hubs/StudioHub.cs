using Microsoft.AspNetCore.SignalR;

namespace AiContentFactory.Api.Hubs;

public sealed class StudioHub : Hub
{
    public async Task JoinWorkspace(string workspaceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId);
    }

    public async Task LeaveWorkspace(string workspaceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId);
    }
}
