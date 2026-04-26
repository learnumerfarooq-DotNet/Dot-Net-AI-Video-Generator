using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.SignalR;

namespace AiContentFactory.Api.Hubs;

public sealed class SignalRNotificationService(IHubContext<StudioHub> hubContext) : IWorkspaceNotificationService
{
    public Task NotifyVideoStageChangedAsync(Guid videoId, string stage, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("OnVideoStageChanged", videoId, stage, cancellationToken);
    }

    public Task NotifyAgentRunStartedAsync(Guid runId, string agentKey, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("OnAgentRunStarted", runId, agentKey, cancellationToken);
    }

    public Task NotifyAgentRunCompletedAsync(Guid runId, string agentKey, string status, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("OnAgentRunCompleted", runId, agentKey, status, cancellationToken);
    }

    public Task NotifyMemoryAddedAsync(Guid memoryId, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("OnMemoryAdded", memoryId, cancellationToken);
    }
}
