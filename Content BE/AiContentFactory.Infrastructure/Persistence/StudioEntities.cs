namespace AiContentFactory.Infrastructure.Persistence;

public sealed class StudioAgentEntity
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool RequiresConnection { get; set; }
    public bool SupportsOpenRouter { get; set; }
    public bool IsConnected { get; set; }
    public string SourceVideoPath { get; set; } = string.Empty;
    public int DailyTokenBudget { get; set; }
    public decimal MonthlyCostBudget { get; set; }
    public string StorageFolderId { get; set; } = string.Empty;
    public string StorageFolderName { get; set; } = string.Empty;
    public string StorageFolderPath { get; set; } = string.Empty;
    public string StorageFolderUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CapabilitySummary { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? SystemPromptTemplateId { get; set; }
    public string? DecisionOutputSchema { get; set; }
}

public sealed class StudioAgentUsageEntity
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; }
    public int RequestCount { get; set; }
    public int TokensIn { get; set; }
    public int TokensOut { get; set; }
    public decimal CostUsd { get; set; }
    public int DurationMs { get; set; }
}

public sealed class StudioGlobalMemoryEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string[] Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public float[]? Embedding { get; set; }
}

public sealed class StudioAgentMemoryEntity
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string[] Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public float[]? Embedding { get; set; }
}

public sealed class StudioVideoEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string StorageFolder { get; set; } = string.Empty;
    public string? DriveFileId { get; set; }
    public string SourceAgentKey { get; set; } = string.Empty;
    public string[] Platforms { get; set; } = Array.Empty<string>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public sealed class StudioPublicationEntity
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PublishedUrl { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public StudioVideoEntity? Video { get; set; }
}

public sealed class StudioScheduleJobEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? AgentKey { get; set; }
    public bool IsEnabled { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string QueueMode { get; set; } = string.Empty;
    public DateTimeOffset? NextRunAt { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class StudioChatMessageEntity
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? DecisionId { get; set; }
    public bool IsStructuredOutput { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class StudioAgentRunEntity
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ExecutionLog { get; set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int MaxRetries { get; set; } = 3;
    public int AttemptCount { get; set; }
}

public sealed class StudioDriveConfigEntity
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string RootFolderId { get; set; } = string.Empty;
    public int PollingInterval { get; set; } = 30;
    public bool AutoCreateFolders { get; set; } = true;
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class StudioAgentConnectionEntity
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool UseOpenRouter { get; set; }
    public string OpenRouterModel { get; set; } = string.Empty;
    public string OpenRouterApiKey { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}
