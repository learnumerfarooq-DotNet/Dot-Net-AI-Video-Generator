using AiContentFactory.Domain.Backlog;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Application.ContentFactory;

public interface IContentFactoryFacade
{
    Task<BrainRunResponse> RunTaskAsync(CreateContentTaskRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemoryEntry>> SearchMemoryAsync(MemorySearchRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemorySuggestion>> GetMemorySuggestionsAsync(CancellationToken cancellationToken);

    Task<MemoryEntry?> ApproveMemorySuggestionAsync(Guid id, ApproveMemorySuggestionRequest request, CancellationToken cancellationToken);

    Task<bool> RejectMemorySuggestionAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<BacklogItem>> GetBacklogAsync(BacklogStatus? status, CancellationToken cancellationToken);

    Task<BacklogItem?> PromoteBacklogAsync(Guid id, CancellationToken cancellationToken);

    Task<SavedProviderState> GetProviderStateAsync(CancellationToken cancellationToken);

    Task<SavedProviderState> SaveProviderStateAsync(SaveProviderCredentialsRequest request, CancellationToken cancellationToken);

    IReadOnlyList<ProviderRequirement> GetProviderRequirements(string? providerType, string? providerName);
}
