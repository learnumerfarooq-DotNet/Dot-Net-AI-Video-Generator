namespace AiContentFactory.Application.Studio;

public interface IStudioWorkspaceFacade
{
    Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken);
    Task<DashboardWorkspaceDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
    Task<PaginatedListDto<VideoItemDto>> GetVideosByStageAsync(string stage, int page, int pageSize, CancellationToken cancellationToken);
    Task<PaginatedListDto<AgentRunDto>> GetAgentRunsAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<AgentChatResponse> SendAgentMessageAsync(string agentKey, SendAgentMessageRequest request, CancellationToken cancellationToken);
    IAsyncEnumerable<AgentStreamChunk> StreamAgentMessageAsync(string agentKey, SendAgentMessageRequest request, CancellationToken cancellationToken);

    Task<MemoryRecordDto?> ApproveMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken);

    Task<MemoryRecordDto?> RejectMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken);

    Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken);

    Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken);

    Task<AgentSettingsDto?> SaveAgentSettingsAsync(string agentKey, SaveAgentSettingsRequest request, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken);

    Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<DriveFileDto>> ListDriveFilesAsync(string? folderId, CancellationToken cancellationToken);

    Task<DriveFileDto?> CreateDriveFolderAsync(string? folderId, string folderName, CancellationToken cancellationToken);
    Task<DriveFileDto?> UploadDriveFileAsync(string? folderId, string fileName, string contentType, Stream fileStream, CancellationToken cancellationToken);
    Task<(Stream Content, string ContentType, string FileName)?> DownloadDriveFileAsync(string fileId, CancellationToken cancellationToken);
    Task<string> RegisterDriveWebhookAsync(string folderId, string webhookUrl, CancellationToken cancellationToken);
    
    Task<ConnectionTestResult> TestAgentConnectionAsync(string agentKey, CancellationToken cancellationToken);
    
    Task<ConnectionTestResult> TestDriveConnectionAsync(CancellationToken cancellationToken);

    Task<VideoItemDto?> LinkVideoToAssetAsync(Guid id, string driveFileId, CancellationToken cancellationToken);
}

public interface IStudioWorkspaceStore
{
    Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken);
    Task<DashboardWorkspaceDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
    Task<PaginatedListDto<VideoItemDto>> GetVideosByStageAsync(string stage, int page, int pageSize, CancellationToken cancellationToken);
    Task<PaginatedListDto<AgentRunDto>> GetAgentRunsAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<AgentConversationContextDto?> GetAgentContextAsync(string agentKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatMessageDto>> SaveAgentExchangeAsync(
        string agentKey,
        string userMessage,
        string assistantMessage,
        int tokensIn,
        int tokensOut,
        decimal cost,
        int durationMs,
        CancellationToken cancellationToken);

    Task<bool> IsAgentWithinBudgetAsync(string agentKey, CancellationToken cancellationToken);

    Task<MemoryRecordDto?> ReviewMemoryAsync(
        Guid id,
        string status,
        ReviewMemoryRequest request,
        CancellationToken cancellationToken);

    Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken);

    Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken);

    Task<AgentSettingsDto?> SaveAgentSettingsAsync(
        string agentKey,
        SaveAgentSettingsRequest request,
        CancellationToken cancellationToken);

    Task<AgentSettingsDto?> GetAgentSettingsAsync(string agentKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken);

    Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken);

    Task<DriveSettingsDto> GetDriveSettingsAsync(CancellationToken cancellationToken);

    Task<VideoItemDto?> LinkVideoToAssetAsync(Guid id, string driveFileId, CancellationToken cancellationToken);
}
