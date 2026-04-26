using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class ScriptAgent(
    ITextGenerationProvider textGenerationProvider,
    IMemoryRepository memoryRepository) : IContentAgent
{
    public string Name => "ScriptAgent";

    public bool CanHandle(CreateContentTaskRequest request) => true;

    public async Task<AgentRunResult> RunAsync(
        CreateContentTaskRequest request,
        IReadOnlyList<MemoryEntry> memories,
        CancellationToken cancellationToken)
    {
        var script = await textGenerationProvider.GenerateScriptAsync(request, memories, cancellationToken);

        if (request.AutoSaveLocalMemory)
        {
            await memoryRepository.SaveLocalAsync(
                Name,
                $"Generated {request.Format} script for {request.Platform} topic '{request.Topic}'.",
                ["script", request.Platform, request.Format],
                null,
                cancellationToken);
        }

        return new AgentRunResult(
            Name,
            Domain.Agents.AgentRunStatus.Succeeded,
            "Script generated and added to the run artifacts.",
            [script]);
    }
}
