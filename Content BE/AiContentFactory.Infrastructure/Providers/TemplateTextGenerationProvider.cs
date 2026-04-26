using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class TemplateTextGenerationProvider(IProviderConfigurationRepository providerConfigurationRepository) : ITextGenerationProvider
{
    public async Task<ContentArtifact> GenerateScriptAsync(
        CreateContentTaskRequest request,
        IReadOnlyList<MemoryEntry> memories,
        CancellationToken cancellationToken)
    {
        var providerConfig = await providerConfigurationRepository.GetAsync(cancellationToken);
        var hook = memories.FirstOrDefault(memory => memory.Tags.Contains("hook"))?.Content
            ?? $"What if {request.Topic} is the shift everyone notices too late?";

        var audience = string.IsNullOrWhiteSpace(request.Audience) ? "curious viewers" : request.Audience;
        var goal = string.IsNullOrWhiteSpace(request.Goal) ? "inform and retain attention" : request.Goal;

        var body = $"""
        Hook:
        {hook}

        Audience:
        {audience}

        Goal:
        {goal}

        Script:
        1. Open with the problem behind "{request.Topic}" in one punchy sentence.
        2. Explain why it is trending now and what changed recently.
        3. Give three concrete points with examples.
        4. Close with a clear takeaway and a platform-native call to action for {request.Platform}.

        Production Notes:
        Format this as {request.Format}. Keep pacing fast, sentences short, and each visual beat easy to turn into a shot.

        Provider:
        Text provider selected for this run: {providerConfig.TextProvider}.
        """;

        return new ContentArtifact(
            Guid.NewGuid(),
            "script",
            $"{request.Topic} - {request.Format} script",
            body,
            DateTimeOffset.UtcNow);
    }
}
