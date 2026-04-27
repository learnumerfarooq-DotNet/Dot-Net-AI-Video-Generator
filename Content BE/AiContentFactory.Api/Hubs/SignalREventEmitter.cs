using AiContentFactory.Application.Common;
using AiContentFactory.Domain.Events;
using Microsoft.AspNetCore.SignalR;

namespace AiContentFactory.Api.Hubs;

public sealed class SignalREventEmitter(IHubContext<StudioHub> hubContext) : IRealtimeEventEmitter
{
    // Brain Events
    public Task EmitBrainTickAsync(BrainTickPayload payload, CancellationToken ct = default) 
        => hubContext.Clients.Group("brain").SendAsync("BrainTickCompleted", payload, ct);

    public Task EmitBrainStatusChangedAsync(BrainStatusPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("brain").SendAsync("BrainStatusChanged", payload, ct);

    public Task EmitGlobalMemorySyncedAsync(MemorySyncedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("brain").SendAsync("GlobalMemorySynced", payload, ct);

    public Task EmitCircuitBreakerStateChangedAsync(CircuitBreakerPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("CircuitBreakerStateChanged", payload, ct);

    public Task EmitBrainPausedResumedAsync(BrainPausePayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("brain").SendAsync("BrainPausedResumed", payload, ct);

    // Pipeline Events
    public Task EmitJobStartedAsync(JobStartedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("JobStarted", payload, ct);

    public Task EmitStageCompletedAsync(StageCompletedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("StageCompleted", payload, ct);

    public Task EmitProgressUpdatedAsync(ProgressUpdatedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("ProgressUpdated", payload, ct);

    public Task EmitJobCompletedAsync(JobCompletedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("JobCompleted", payload, ct);

    public Task EmitJobFailedAsync(JobFailedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("JobFailed", payload, ct);

    public Task EmitJobRetryingAsync(JobRetryingPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("JobRetrying", payload, ct);

    // Agent Events
    public Task EmitAgentDispatchedAsync(AgentDispatchedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"agent:{payload.AgentKey}").SendAsync("AgentDispatched", payload, ct);

    public Task EmitAgentRunStartedAsync(AgentRunStartedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"agent:{payload.AgentKey}").SendAsync("AgentRunStarted", payload, ct);

    public Task EmitAgentRunCompletedAsync(AgentRunCompletedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"agent:{payload.AgentKey}").SendAsync("AgentRunCompleted", payload, ct);

    public Task EmitAgentHealthChangedAsync(AgentHealthPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("AgentHealthChanged", payload, ct);

    public Task EmitAgentChatResponseAsync(AgentChatResponsePayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"agent:{payload.AgentKey}").SendAsync("AgentChatResponse", payload, ct);

    public Task EmitAgentChatStreamChunkAsync(AgentChatStreamPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"agent:{payload.AgentKey}").SendAsync("AgentChatStreamChunk", payload, ct);

    // Content Events
    public Task EmitScriptGeneratedAsync(ScriptGeneratedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("ScriptGenerated", payload, ct);

    public Task EmitVideoEditedAsync(VideoEditedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("VideoEdited", payload, ct);

    public Task EmitShortsCreatedAsync(ShortsCreatedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("ShortsCreated", payload, ct);

    public Task EmitShortEditCompletedAsync(ShortEditCompletedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("pipeline").SendAsync("ShortEditCompleted", payload, ct);

    public Task EmitUploadPackageReadyAsync(UploadPackagePayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("publishing").SendAsync("UploadPackageReady", payload, ct);

    // Publishing Events
    public Task EmitUploadStartedAsync(UploadStartedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("publishing").SendAsync("UploadStarted", payload, ct);

    public Task EmitUploadProgressAsync(UploadProgressPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("publishing").SendAsync("UploadProgress", payload, ct);

    public Task EmitUploadCompletedAsync(UploadCompletedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("publishing").SendAsync("UploadCompleted", payload, ct);

    public Task EmitUploadFailedAsync(UploadFailedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.Group("publishing").SendAsync("UploadFailed", payload, ct);

    // System Events
    public Task EmitTrendDiscoveryCompleteAsync(TrendDiscoveryPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("TrendDiscoveryComplete", payload, ct);

    public Task EmitAnalyticsReportReadyAsync(AnalyticsReportPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("AnalyticsReportReady", payload, ct);

    public Task EmitMemorySuggestionCreatedAsync(MemorySuggestionPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("MemorySuggestionCreated", payload, ct);

    public Task EmitDriveFileDetectedAsync(DriveFileDetectedPayload payload, CancellationToken ct = default)
        => hubContext.Clients.All.SendAsync("DriveFileDetected", payload, ct);
}
