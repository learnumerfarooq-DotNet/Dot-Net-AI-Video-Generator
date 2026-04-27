using Microsoft.AspNetCore.SignalR;

namespace AiContentFactory.Api.Hubs;

public sealed class StudioHub : Hub
{
    public async Task SubscribeToBrain() => await Groups.AddToGroupAsync(Context.ConnectionId, "brain");
    public async Task UnsubscribeFromBrain() => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "brain");

    public async Task SubscribeToPipeline() => await Groups.AddToGroupAsync(Context.ConnectionId, "pipeline");
    public async Task UnsubscribeFromPipeline() => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "pipeline");

    public async Task SubscribeToAgent(string agentKey) => await Groups.AddToGroupAsync(Context.ConnectionId, $"agent:{agentKey}");
    public async Task UnsubscribeFromAgent(string agentKey) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent:{agentKey}");

    public async Task SubscribeToPublishing() => await Groups.AddToGroupAsync(Context.ConnectionId, "publishing");
    public async Task UnsubscribeFromPublishing() => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "publishing");
}
