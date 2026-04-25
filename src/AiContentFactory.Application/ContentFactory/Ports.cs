using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Backlog;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Application.ContentFactory;

public interface IMemoryRepository
{
    Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemorySearchRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemorySuggestion>> GetPendingSuggestionsAsync(CancellationToken cancellationToken);

    Task<MemorySuggestion> SuggestAsync(MemorySuggestion suggestion, CancellationToken cancellationToken);

    Task<MemoryEntry?> ApproveSuggestionAsync(Guid suggestionId, string? revisedContent, CancellationToken cancellationToken);

    Task<bool> RejectSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken);

    Task<MemoryEntry> SaveLocalAsync(string agentName, string content, IReadOnlyList<string> tags, CancellationToken cancellationToken);
}

public interface IBacklogRepository
{
    Task<IReadOnlyList<BacklogItem>> ListAsync(BacklogStatus? status, CancellationToken cancellationToken);

    Task<BacklogItem> AddAsync(BacklogItem item, CancellationToken cancellationToken);

    Task<BacklogItem?> UpdateStatusAsync(Guid id, BacklogStatus status, CancellationToken cancellationToken);
}

public interface IProviderConfigurationRepository
{
    Task<ProviderConfig> GetAsync(CancellationToken cancellationToken);

    Task<ProviderConfig> UpdateAsync(ProviderConfig config, CancellationToken cancellationToken);
}

public interface IProviderCredentialRepository
{
    Task SaveManyAsync(IReadOnlyList<ProviderCredentialInput> credentials, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProviderCredentialStatus>> GetStatusAsync(CancellationToken cancellationToken);
}

public interface IProviderRequirementCatalog
{
    IReadOnlyList<ProviderRequirement> List();
}

public interface IContentAgent
{
    string Name { get; }

    bool CanHandle(CreateContentTaskRequest request);

    Task<AgentRunResult> RunAsync(
        CreateContentTaskRequest request,
        IReadOnlyList<MemoryEntry> memories,
        CancellationToken cancellationToken);
}

public interface ITextGenerationProvider
{
    Task<ContentArtifact> GenerateScriptAsync(
        CreateContentTaskRequest request,
        IReadOnlyList<MemoryEntry> memories,
        CancellationToken cancellationToken);
}

public interface IUploadExecutionProvider
{
    Task<string> UploadAsync(BacklogItem item, CancellationToken cancellationToken);
}
