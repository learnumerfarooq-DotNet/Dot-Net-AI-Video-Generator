namespace AiContentFactory.Application.Studio;

public interface IWorkspaceNotificationService
{
    Task NotifyVideoStageChangedAsync(Guid videoId, string stage, CancellationToken cancellationToken);
    Task NotifyAgentRunStartedAsync(Guid runId, string agentKey, CancellationToken cancellationToken);
    Task NotifyAgentRunCompletedAsync(Guid runId, string agentKey, string status, CancellationToken cancellationToken);
    Task NotifyMemoryAddedAsync(Guid memoryId, CancellationToken cancellationToken);
}
