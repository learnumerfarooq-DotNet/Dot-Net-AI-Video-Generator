using AiContentFactory.Domain.Backlog;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Application.ContentFactory;

public sealed class ContentFactoryFacade(
    IEnumerable<IContentAgent> agents,
    IMemoryRepository memoryRepository,
    IBacklogRepository backlogRepository,
    IProviderConfigurationRepository providerConfigurationRepository,
    IProviderCredentialRepository providerCredentialRepository,
    IProviderRequirementCatalog providerRequirementCatalog) : IContentFactoryFacade
{
    public async Task<BrainRunResponse> RunTaskAsync(CreateContentTaskRequest request, CancellationToken cancellationToken)
    {
        var globalMemories = await memoryRepository.SearchAsync(new MemorySearchRequest(MemoryScope.Global, null), cancellationToken);
        var localMemories = await memoryRepository.SearchAsync(new MemorySearchRequest(MemoryScope.Local, null), cancellationToken);
        var memories = globalMemories.Concat(localMemories).Take(20).ToArray();

        var plan = new[]
        {
            "Search global and local memory before execution.",
            "Generate a platform-aware script artifact.",
            "Prepare upload metadata/checklist in dry-run mode.",
            "Save artifacts to backlog for pause/resume workflow.",
            "Suggest global memory updates for human approval."
        };

        var results = new List<AgentRunResult>();
        foreach (var agent in agents.Where(agent => agent.CanHandle(request)))
        {
            results.Add(await agent.RunAsync(request, memories, cancellationToken));
        }

        var artifacts = results.SelectMany(result => result.Artifacts).ToArray();
        var now = DateTimeOffset.UtcNow;

        await backlogRepository.AddAsync(new BacklogItem(
            Guid.NewGuid(),
            request.Topic,
            request.Platform,
            request.Format,
            BacklogStatus.Backlog,
            artifacts,
            now,
            now), cancellationToken);

        var suggestion = await memoryRepository.SuggestAsync(new MemorySuggestion(
            Guid.NewGuid(),
            MemoryScope.Global,
            null,
            $"Topic '{request.Topic}' produced {artifacts.Length} artifacts for {request.Platform}/{request.Format}. Review performance after publishing.",
            "Global learning must be approved by Brain/operator before becoming shared memory.",
            MemorySuggestionStatus.Pending,
            now), cancellationToken);

        return new BrainRunResponse(
            Guid.NewGuid(),
            request.Topic,
            plan,
            results,
            memories,
            [suggestion],
            now);
    }

    public Task<IReadOnlyList<MemoryEntry>> SearchMemoryAsync(MemorySearchRequest request, CancellationToken cancellationToken)
        => memoryRepository.SearchAsync(request, cancellationToken);

    public Task<IReadOnlyList<MemorySuggestion>> GetMemorySuggestionsAsync(CancellationToken cancellationToken)
        => memoryRepository.GetPendingSuggestionsAsync(cancellationToken);

    public Task<MemoryEntry?> ApproveMemorySuggestionAsync(Guid id, ApproveMemorySuggestionRequest request, CancellationToken cancellationToken)
        => memoryRepository.ApproveSuggestionAsync(id, request.RevisedContent, cancellationToken);

    public Task<bool> RejectMemorySuggestionAsync(Guid id, CancellationToken cancellationToken)
        => memoryRepository.RejectSuggestionAsync(id, cancellationToken);

    public Task<IReadOnlyList<BacklogItem>> GetBacklogAsync(BacklogStatus? status, CancellationToken cancellationToken)
        => backlogRepository.ListAsync(status, cancellationToken);

    public Task<BacklogItem?> PromoteBacklogAsync(Guid id, CancellationToken cancellationToken)
        => backlogRepository.UpdateStatusAsync(id, BacklogStatus.Ready, cancellationToken);

    public async Task<SavedProviderState> GetProviderStateAsync(CancellationToken cancellationToken)
    {
        var config = await providerConfigurationRepository.GetAsync(cancellationToken);
        var status = await providerCredentialRepository.GetStatusAsync(cancellationToken);
        return new SavedProviderState(config, status);
    }

    public async Task<SavedProviderState> SaveProviderStateAsync(SaveProviderCredentialsRequest request, CancellationToken cancellationToken)
    {
        var config = await providerConfigurationRepository.UpdateAsync(request.Config, cancellationToken);
        await providerCredentialRepository.SaveManyAsync(request.Credentials, cancellationToken);
        var status = await providerCredentialRepository.GetStatusAsync(cancellationToken);
        return new SavedProviderState(config, status);
    }

    public IReadOnlyList<ProviderRequirement> GetProviderRequirements(string? providerType, string? providerName)
    {
        return providerRequirementCatalog.List()
            .Where(requirement => string.IsNullOrWhiteSpace(providerType) ||
                string.Equals(requirement.ProviderType, providerType, StringComparison.OrdinalIgnoreCase))
            .Where(requirement => string.IsNullOrWhiteSpace(providerName) ||
                string.Equals(requirement.ProviderName, providerName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
