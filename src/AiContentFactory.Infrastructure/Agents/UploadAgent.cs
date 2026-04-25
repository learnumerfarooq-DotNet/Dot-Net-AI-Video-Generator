using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class UploadAgent(IProviderConfigurationRepository providerConfigurationRepository) : IContentAgent
{
    public string Name => "UploadAgent";

    public bool CanHandle(CreateContentTaskRequest request)
    {
        return string.Equals(request.Platform, "youtube", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Platform, "tiktok", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Platform, "instagram", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Platform, "linkedin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<AgentRunResult> RunAsync(
        CreateContentTaskRequest request,
        IReadOnlyList<MemoryEntry> memories,
        CancellationToken cancellationToken)
    {
        var providerConfig = await providerConfigurationRepository.GetAsync(cancellationToken);
        var notes = new ContentArtifact(
            Guid.NewGuid(),
            "upload-plan",
            $"{request.Platform} upload checklist",
            $"Prepare title, description, tags, thumbnail, and scheduled upload window for '{request.Topic}'. Upload provider selected: {providerConfig.UploadProvider}.",
            DateTimeOffset.UtcNow);

        return new AgentRunResult(
            Name,
            Domain.Agents.AgentRunStatus.Succeeded,
            $"Upload checklist prepared with {providerConfig.UploadProvider} selected.",
            [notes]);
    }
}
