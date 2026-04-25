using System.Text;
using AiContentFactory.Application.ContentFactory;

namespace AiContentFactory.Application.Studio;

public sealed class StudioWorkspaceFacade(
    IStudioWorkspaceStore store,
    IEnumerable<IChatProvider> chatProviders) : IStudioWorkspaceFacade
{
    public Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken)
        => store.GetBootstrapAsync(cancellationToken);

    public async Task<AgentChatResponse> SendAgentMessageAsync(
        string agentKey,
        SendAgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new AgentChatResponse(true, "Send a message before starting the chat.", []);
        }

        var context = await store.GetAgentContextAsync(agentKey, cancellationToken);
        if (context is null)
        {
            return new AgentChatResponse(true, "This agent is not available yet.", []);
        }

        var settings = await store.GetAgentSettingsAsync(agentKey, cancellationToken);
        
        var assistantReply = string.Empty;
        var provider = chatProviders.FirstOrDefault(p => string.Equals(p.ProviderName, context.Agent.ProviderName, StringComparison.OrdinalIgnoreCase));

        if (provider != null && context.Agent.IsConnected && settings != null)
        {
            try
            {
                var systemPrompt = BuildSystemPrompt(context);
                var history = context.Messages.Select(m => new ChatMessageInput(m.Role, m.Content)).ToList();
                
                var apiKey = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterApiKey)
                    ? settings.OpenRouterApiKey 
                    : settings.ApiKey;
                    
                var model = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterModel)
                    ? settings.OpenRouterModel
                    : settings.ModelName;
                    
                var result = await provider.CompleteAsync(
                    new ChatCompletionRequest(systemPrompt, history, model),
                    apiKey,
                    settings.BaseUrl,
                    cancellationToken);
                    
                assistantReply = result.Content;
            }
            catch (Exception ex)
            {
                assistantReply = $"[Provider Error] Failed to contact {context.Agent.ProviderName}: {ex.Message}\n\nFalling back to workspace-preview...\n\n{BuildAgentReply(request.Message, context)}";
            }
        }
        else
        {
            assistantReply = BuildAgentReply(request.Message, context);
        }

        var messages = await store.SaveAgentExchangeAsync(agentKey, request.Message, assistantReply, cancellationToken);

        return new AgentChatResponse(
            false,
            context.Agent.RequiresConnection && !context.Agent.IsConnected
                ? $"{context.Agent.Name} responded in workspace-preview mode. Save credentials in Settings when you are ready for live execution."
                : $"{context.Agent.Name} responded successfully.",
            messages);
    }

    public Task<MemoryRecordDto?> ApproveMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken)
        => store.ReviewMemoryAsync(id, "Approved", request, cancellationToken);

    public Task<MemoryRecordDto?> RejectMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken)
        => store.ReviewMemoryAsync(id, "Rejected", request, cancellationToken);

    public Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken)
        => store.UpdateVideoStageAsync(id, request, cancellationToken);

    public Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken)
        => store.CreateManualScheduleAsync(request, cancellationToken);

    public Task<AgentSettingsDto?> SaveAgentSettingsAsync(string agentKey, SaveAgentSettingsRequest request, CancellationToken cancellationToken)
        => store.SaveAgentSettingsAsync(agentKey, request, cancellationToken);

    public Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken)
        => store.GetPendingMemorySuggestionsAsync(cancellationToken);

    private static string BuildAgentReply(string userMessage, AgentConversationContextDto context)
    {
        var globalHints = context.GlobalMemories.Take(2).Select(memory => memory.Title).ToArray();
        var localHints = context.LocalMemories.Take(2).Select(memory => memory.Title).ToArray();

        var builder = new StringBuilder();
        builder.AppendLine($"{context.Agent.Name} response");
        builder.AppendLine();
        builder.AppendLine($"Focus request: {userMessage}");
        builder.AppendLine();

        switch (context.Agent.Category)
        {
            case "Brain":
                builder.AppendLine("Recommended next move:");
                builder.AppendLine($"1. Coordinate Trend, Script, and Video agents using the current queue. Ready videos: {context.ReadyVideos.Count}, backlog videos: {context.BacklogVideos.Count}.");
                builder.AppendLine("2. Keep the topic technical, concise, and directly tied to Angular/.NET architecture tradeoffs.");
                builder.AppendLine("3. Move reusable learnings to global memory only after review.");
                break;
            case "Discovery":
                builder.AppendLine("Trend plan:");
                builder.AppendLine("1. Rank topics by technical urgency, search interest, and social repackaging potential.");
                builder.AppendLine("2. Prefer subjects where Angular, .NET, and AI workflow improvements intersect.");
                builder.AppendLine("3. Push only the strongest angles into the scheduler queue.");
                break;
            case "Writing":
                builder.AppendLine("Script plan:");
                builder.AppendLine("1. Lead with one painful engineering problem before naming the solution.");
                builder.AppendLine("2. Use three concrete points with folder structure, code decisions, and tradeoffs.");
                builder.AppendLine("3. End with a sharp takeaway that fits the target platform.");
                break;
            case "Video":
                builder.AppendLine("Video production plan:");
                builder.AppendLine("1. Break the script into visual beats with IDE closeups and clean motion.");
                builder.AppendLine("2. Reuse the Google Drive asset folders before generating new shots.");
                builder.AppendLine("3. Render for the platform mix already attached to the video queue.");
                break;
            case "Shorts":
                builder.AppendLine("Shorts plan:");
                builder.AppendLine("1. Compress the idea into one pain point, one shift, and one payoff.");
                builder.AppendLine("2. Write 3 hook variants and keep each version under a single clear angle.");
                builder.AppendLine("3. Repackage the best version per platform instead of cross-posting identical copy.");
                break;
            default:
                builder.AppendLine("Publishing plan:");
                builder.AppendLine("1. Match the title, caption, and timing to the platform’s audience behavior.");
                builder.AppendLine("2. Check retries, posting window, and storage folder before publishing.");
                builder.AppendLine("3. Store the performance result back into the memory workflow after release.");
                break;
        }

        if (globalHints.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine($"Global memory signals: {string.Join(", ", globalHints)}.");
        }

        if (localHints.Length > 0)
        {
            builder.AppendLine($"Local agent hints: {string.Join(", ", localHints)}.");
        }

        if (context.Agent.RequiresConnection && !context.Agent.IsConnected)
        {
            builder.AppendLine();
            builder.AppendLine("Connection note: this is a workspace-preview reply. Add provider credentials in Settings before triggering live execution.");
        }

        builder.AppendLine();
        builder.AppendLine($"Agent capability: {context.Agent.CapabilitySummary}");

        return builder.ToString();
    }

    private static string BuildSystemPrompt(AgentConversationContextDto context)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"You are an expert AI agent named '{context.Agent.Name}' with the following capabilities: {context.Agent.CapabilitySummary}.");
        builder.AppendLine("You are part of an advanced AI multi-agent Video Factory that operates using a 'Self-Improving Loop' (Create -> Execute -> Upload -> Analyze -> Improve).");
        builder.AppendLine();
        
        var globalHints = context.GlobalMemories.Select(memory => memory.Title).ToArray();
        var localHints = context.LocalMemories.Select(memory => memory.Title).ToArray();

        if (globalHints.Length > 0)
        {
            builder.AppendLine("=== Global Architecture Memory ===");
            foreach(var hint in globalHints) builder.AppendLine($"- {hint}");
            builder.AppendLine();
        }

        if (localHints.Length > 0)
        {
            builder.AppendLine("=== Local Agent Memory ===");
            foreach(var hint in localHints) builder.AppendLine($"- {hint}");
            builder.AppendLine();
        }

        builder.AppendLine("Please read the user's focus request and provide a concise, highly technical action plan. Output in clear markdown.");
        return builder.ToString();
    }
}
