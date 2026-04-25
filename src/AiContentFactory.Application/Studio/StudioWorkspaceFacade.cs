using System.Text;
using AiContentFactory.Application.ContentFactory;

namespace AiContentFactory.Application.Studio;

public sealed class StudioWorkspaceFacade(
    IStudioWorkspaceStore store,
    IGoogleDriveService driveService,
    IEnumerable<IChatProvider> chatProviders) : IStudioWorkspaceFacade
{
    public async Task<IReadOnlyList<DriveFileDto>> ListDriveFilesAsync(CancellationToken cancellationToken)
    {
        var settings = await store.GetDriveSettingsAsync(cancellationToken);
        return await driveService.ListFilesAsync(settings, cancellationToken);
    }

    public async Task<DriveFileDto?> CreateDriveFolderAsync(string folderName, CancellationToken cancellationToken)
    {
        var settings = await store.GetDriveSettingsAsync(cancellationToken);
        return await driveService.CreateFolderAsync(settings, folderName, cancellationToken);
    }

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
        var effectiveProviderName = settings?.UseOpenRouter == true ? "OpenRouter" : context.Agent.ProviderName;
        var provider = chatProviders.FirstOrDefault(p => string.Equals(p.ProviderName, effectiveProviderName, StringComparison.OrdinalIgnoreCase));

        if (provider != null && context.Agent.IsConnected && settings != null)
        {
            try
            {
                var systemPrompt = BuildSystemPrompt(context);
                
                // Build history from past messages, filtering out broken/debug responses
                var history = context.Messages
                    .Where(m => !string.IsNullOrWhiteSpace(m.Content) 
                             && !m.Content.StartsWith("No response content generated"))
                    .Select(m => new ChatMessageInput(m.Role, m.Content))
                    .ToList();
                
                // CRITICAL: Append the current user message — it hasn't been saved to DB yet
                history.Add(new ChatMessageInput("user", request.Message));
                
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

    public Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken)
        => store.SaveDriveSettingsAsync(request, cancellationToken);

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
        builder.AppendLine($"You are '{context.Agent.Name}', an intelligent AI assistant.");
        builder.AppendLine($"Your specialty: {context.Agent.CapabilitySummary}.");
        builder.AppendLine();
        builder.AppendLine("You are part of a multi-agent AI Video Content Factory. Your role is to help the user with anything they ask — whether it's casual conversation, technical questions, content strategy, or workflow decisions.");
        builder.AppendLine();
        builder.AppendLine("Guidelines:");
        builder.AppendLine("- Be natural, friendly, and conversational. Match the user's tone.");
        builder.AppendLine("- If the user asks a general question (e.g. 'hi', 'how are you', 'what's your name'), respond naturally like a helpful assistant. Do NOT force workflow updates into casual replies.");
        builder.AppendLine("- If the user asks about the workspace, videos, agents, or content pipeline, then reference the workspace context below.");
        builder.AppendLine("- Use markdown formatting when giving detailed or technical responses.");
        builder.AppendLine();
        
        // Workspace context (only included as reference, not forced into every reply)
        builder.AppendLine("=== Workspace Context (reference only, use when relevant) ===");
        builder.AppendLine($"Ready videos: {context.ReadyVideos.Count}, Backlog videos: {context.BacklogVideos.Count}");
        
        var globalHints = context.GlobalMemories.Select(memory => memory.Title).Take(3).ToArray();
        var localHints = context.LocalMemories.Select(memory => memory.Title).Take(3).ToArray();

        if (globalHints.Length > 0)
        {
            builder.AppendLine($"Global memory: {string.Join("; ", globalHints)}");
        }

        if (localHints.Length > 0)
        {
            builder.AppendLine($"Local memory: {string.Join("; ", localHints)}");
        }

        return builder.ToString();
    }
}
