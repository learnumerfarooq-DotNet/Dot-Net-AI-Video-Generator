namespace AiContentFactory.Application.Studio;

public sealed record PaginatedListDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record WorkspaceBootstrapResponse(
    DashboardWorkspaceDto Dashboard,
    AgentWorkspaceDto Agents,
    MemoryWorkspaceDto Memory,
    SchedulerWorkspaceDto Scheduler,
    SettingsWorkspaceDto Settings,
    DriveSettingsDto Drive,
    DateTimeOffset GeneratedAt);

public sealed record DashboardWorkspaceDto(
    IReadOnlyList<UsageSeriesDto> UsageSeries,
    MemoryCountsDto MemoryCounts,
    IReadOnlyList<VideoItemDto> ReadyVideos,
    IReadOnlyList<VideoItemDto> BacklogVideos,
    IReadOnlyList<PlatformPublicationWidgetDto> PublishedWidgets,
    IReadOnlyList<VideoItemDto> RecentlyPublished,
    IReadOnlyList<AgentRunDto> RecentRuns);

public sealed record AgentWorkspaceDto(
    IReadOnlyList<AgentSummaryDto> Agents,
    IReadOnlyList<ChatMessageDto> ChatMessages);

public sealed record MemoryWorkspaceDto(
    MemoryCountsDto Counts,
    IReadOnlyList<MemoryRecordDto> ReviewQueue,
    IReadOnlyList<MemoryRecordDto> GlobalMemories,
    IReadOnlyList<MemoryRecordDto> LocalMemories);

public sealed record SchedulerWorkspaceDto(
    IReadOnlyList<ScheduleJobDto> ManualSchedules,
    IReadOnlyList<ScheduleJobDto> DailyPostingJobs,
    IReadOnlyList<ScheduleJobDto> RetryJobs,
    IReadOnlyList<ScheduleJobDto> QueueJobs);

public sealed record SettingsWorkspaceDto(
    IReadOnlyList<AgentSettingsDto> Agents,
    IReadOnlyList<ProviderOptionDto> ProviderOptions);

public sealed record UsageSeriesDto(
    string AgentKey,
    string AgentName,
    string AccentColor,
    IReadOnlyList<UsagePointDto> Points);

public sealed record UsagePointDto(
    DateTimeOffset CapturedAt,
    int RequestCount,
    int TokensIn,
    int TokensOut,
    decimal CostUsd,
    int DurationMs);

public sealed record MemoryCountsDto(
    int GlobalApproved,
    int LocalApproved,
    int PendingReview);

public sealed record PlatformPublicationWidgetDto(
    string Platform,
    int PublishedCount,
    int ScheduledCount,
    int FailedCount,
    long TotalViews);

public sealed record VideoItemDto(
    Guid Id,
    string Title,
    string Topic,
    string Format,
    string Stage,
    string StorageFolder,
    string? DriveFileId,
    string SourceAgentKey,
    IReadOnlyList<string> Platforms,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);

public sealed record AgentRunDto(
    Guid Id,
    string AgentKey,
    string Title,
    string Status,
    string Summary,
    DateTimeOffset QueuedAt,
    DateTimeOffset? CompletedAt,
    int AttemptCount = 1,
    int MaxRetries = 1);

public sealed record AgentSummaryDto(
    string Key,
    string Name,
    string Description,
    string Category,
    bool RequiresConnection,
    bool SupportsOpenRouter,
    bool IsConnected,
    string ProviderName,
    string ModelName,
    string Status,
    string CapabilitySummary,
    DateTimeOffset? LastRunAt,
    IReadOnlyList<string> LocalMemoryHighlights,
    IReadOnlyList<AgentRunDto> RecentRuns);

public sealed record ChatMessageDto(
    Guid Id,
    string AgentKey,
    string Role,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record AgentConversationContextDto(
    AgentSummaryDto Agent,
    IReadOnlyList<ChatMessageDto> Messages,
    IReadOnlyList<MemoryRecordDto> GlobalMemories,
    IReadOnlyList<MemoryRecordDto> LocalMemories,
    IReadOnlyList<VideoItemDto> BacklogVideos,
    IReadOnlyList<VideoItemDto> ReadyVideos);

public sealed record AgentChatResponse(
    bool Blocked,
    string Message,
    IReadOnlyList<ChatMessageDto> Messages);

public sealed record AgentStreamChunk(
    string Type, // 'thought' | 'delta' | 'tool' | 'done'
    string Content,
    ChatMessageDto? Message = null);

public sealed record MemoryRecordDto(
    Guid Id,
    string Scope,
    string? AgentKey,
    string Title,
    string Content,
    string Status,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? ExpiresAt = null);

public sealed record MemorySuggestionDto(
    Guid Id,
    string Scope,
    string? AgentName,
    string Content,
    string Reason,
    string Status,
    DateTimeOffset CreatedAt);
public sealed record ScheduleJobDto(
    Guid Id,
    string Name,
    string Type,
    string? AgentKey,
    bool IsEnabled,
    string Status,
    string Trigger,
    string QueueMode,
    DateTimeOffset? NextRunAt,
    DateTimeOffset? LastRunAt,
    string Notes);

public sealed record AgentSettingsDto(
    string AgentKey,
    string AgentName,
    string Category,
    bool RequiresConnection,
    bool SupportsOpenRouter,
    bool IsConnected,
    string ProviderName,
    string ModelName,
    string BaseUrl,
    string ApiKey,
    string ClientId,
    string ClientSecret,
    string RefreshToken,
    string SourceVideoPath,
    string StorageFolderId,
    string StorageFolderName,
    string StorageFolderPath,
    string StorageFolderUrl,
    bool UseOpenRouter,
    string OpenRouterModel,
    string OpenRouterApiKey,
    string Notes,
    DateTimeOffset UpdatedAt);

public sealed record ProviderOptionDto(
    string Category,
    IReadOnlyList<string> Providers);

public sealed record DriveFileDto(
    string Id,
    string Name,
    string Type,
    string Size,
    string Date);

public sealed record DriveSettingsDto(
    string ClientId,
    string ClientSecret,
    string RefreshToken,
    string RootFolderId);

public sealed record SendAgentMessageRequest(string Message);

public sealed record ReviewMemoryRequest(string? RevisedTitle, string? RevisedContent, int? TtlDays = null);

public sealed record UpdateVideoStageRequest(string Stage);

public sealed record CreateManualScheduleRequest(
    string Name,
    string AgentKey,
    string Trigger,
    string Notes,
    bool IsEnabled = true);

public sealed record SaveAgentSettingsRequest(
    string ProviderName,
    string ModelName,
    string BaseUrl,
    string ApiKey,
    string ClientId,
    string ClientSecret,
    string RefreshToken,
    string SourceVideoPath,
    string StorageFolderId,
    bool UseOpenRouter,
    string OpenRouterModel,
    string OpenRouterApiKey,
    string Notes,
    string StorageFolderName = "",
    string StorageFolderPath = "",
    string StorageFolderUrl = "");

public sealed record SaveDriveSettingsRequest(
    string ClientId,
    string ClientSecret,
    string RefreshToken,
    string RootFolderId);

public sealed record ConnectionTestResult(
    bool Success,
    string Message,
    string? Details = null);
