namespace AiContentFactory.Application.Studio;

public interface IStudioWorkspaceFacade
{
    Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken);

    Task<AgentChatResponse> SendAgentMessageAsync(string agentKey, SendAgentMessageRequest request, CancellationToken cancellationToken);

    Task<MemoryRecordDto?> ApproveMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken);

    Task<MemoryRecordDto?> RejectMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken);

    Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken);

    Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken);

    Task<AgentSettingsDto?> SaveAgentSettingsAsync(string agentKey, SaveAgentSettingsRequest request, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken);

    Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<DriveFileDto>> ListDriveFilesAsync(CancellationToken cancellationToken);

    Task<DriveFileDto?> CreateDriveFolderAsync(string folderName, CancellationToken cancellationToken);
}

public interface IStudioWorkspaceStore
{
    Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken);

    Task<AgentConversationContextDto?> GetAgentContextAsync(string agentKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatMessageDto>> SaveAgentExchangeAsync(
        string agentKey,
        string userMessage,
        string assistantMessage,
        CancellationToken cancellationToken);

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
}
