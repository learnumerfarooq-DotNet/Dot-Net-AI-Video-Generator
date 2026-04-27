using AiContentFactory.Domain.Events;

namespace AiContentFactory.Application.Common;

public interface IRealtimeEventEmitter
{
    // Brain Events
    Task EmitBrainTickAsync(BrainTickPayload payload, CancellationToken ct = default);
    Task EmitBrainStatusChangedAsync(BrainStatusPayload payload, CancellationToken ct = default);
    Task EmitGlobalMemorySyncedAsync(MemorySyncedPayload payload, CancellationToken ct = default);
    Task EmitCircuitBreakerStateChangedAsync(CircuitBreakerPayload payload, CancellationToken ct = default);
    Task EmitBrainPausedResumedAsync(BrainPausePayload payload, CancellationToken ct = default);

    // Pipeline Events
    Task EmitJobStartedAsync(JobStartedPayload payload, CancellationToken ct = default);
    Task EmitStageCompletedAsync(StageCompletedPayload payload, CancellationToken ct = default);
    Task EmitProgressUpdatedAsync(ProgressUpdatedPayload payload, CancellationToken ct = default);
    Task EmitJobCompletedAsync(JobCompletedPayload payload, CancellationToken ct = default);
    Task EmitJobFailedAsync(JobFailedPayload payload, CancellationToken ct = default);
    Task EmitJobRetryingAsync(JobRetryingPayload payload, CancellationToken ct = default);

    // Agent Events
    Task EmitAgentDispatchedAsync(AgentDispatchedPayload payload, CancellationToken ct = default);
    Task EmitAgentRunStartedAsync(AgentRunStartedPayload payload, CancellationToken ct = default);
    Task EmitAgentRunCompletedAsync(AgentRunCompletedPayload payload, CancellationToken ct = default);
    Task EmitAgentHealthChangedAsync(AgentHealthPayload payload, CancellationToken ct = default);
    Task EmitAgentChatResponseAsync(AgentChatResponsePayload payload, CancellationToken ct = default);
    Task EmitAgentChatStreamChunkAsync(AgentChatStreamPayload payload, CancellationToken ct = default);

    // Content Events
    Task EmitScriptGeneratedAsync(ScriptGeneratedPayload payload, CancellationToken ct = default);
    Task EmitVideoEditedAsync(VideoEditedPayload payload, CancellationToken ct = default);
    Task EmitShortsCreatedAsync(ShortsCreatedPayload payload, CancellationToken ct = default);
    Task EmitShortEditCompletedAsync(ShortEditCompletedPayload payload, CancellationToken ct = default);
    Task EmitUploadPackageReadyAsync(UploadPackagePayload payload, CancellationToken ct = default);

    // Publishing Events
    Task EmitUploadStartedAsync(UploadStartedPayload payload, CancellationToken ct = default);
    Task EmitUploadProgressAsync(UploadProgressPayload payload, CancellationToken ct = default);
    Task EmitUploadCompletedAsync(UploadCompletedPayload payload, CancellationToken ct = default);
    Task EmitUploadFailedAsync(UploadFailedPayload payload, CancellationToken ct = default);

    // System Events
    Task EmitTrendDiscoveryCompleteAsync(TrendDiscoveryPayload payload, CancellationToken ct = default);
    Task EmitAnalyticsReportReadyAsync(AnalyticsReportPayload payload, CancellationToken ct = default);
    Task EmitMemorySuggestionCreatedAsync(MemorySuggestionPayload payload, CancellationToken ct = default);
    Task EmitDriveFileDetectedAsync(DriveFileDetectedPayload payload, CancellationToken ct = default);
}
