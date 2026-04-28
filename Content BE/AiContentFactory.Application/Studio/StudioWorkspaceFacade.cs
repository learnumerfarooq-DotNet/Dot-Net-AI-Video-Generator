using System.Text;
using AiContentFactory.Application.ContentFactory;
using Microsoft.Extensions.Caching.Memory;

namespace AiContentFactory.Application.Studio;

public sealed class StudioWorkspaceFacade : IStudioWorkspaceFacade
{
    private readonly IStudioWorkspaceStore _store;
    private readonly IGoogleDriveService _driveService;
    private readonly IEnumerable<IChatProvider> _chatProviders;
    private readonly StudioToolRegistry _toolRegistry;
    private readonly IMemoryCache _cache;

    public StudioWorkspaceFacade(
        IStudioWorkspaceStore store,
        IGoogleDriveService driveService,
        IEnumerable<IChatProvider> chatProviders,
        StudioToolRegistry toolRegistry,
        IMemoryCache cache)
    {
        _store = store;
        _driveService = driveService;
        _chatProviders = chatProviders;
        _toolRegistry = toolRegistry;
        _cache = cache;
    }
    public async Task<IReadOnlyList<DriveFileDto>> ListDriveFilesAsync(string? folderId, CancellationToken cancellationToken)
    {
        var settings = await _store.GetDriveSettingsAsync(cancellationToken);
        return await _driveService.ListFilesAsync(settings, folderId, cancellationToken);
    }

    public async Task<DriveFileDto?> CreateDriveFolderAsync(string? folderId, string folderName, CancellationToken cancellationToken)
    {
        var settings = await _store.GetDriveSettingsAsync(cancellationToken);
        return await _driveService.CreateFolderAsync(settings, folderId, folderName, cancellationToken);
    }
    
    public async Task<DriveFileDto?> UploadDriveFileAsync(string? folderId, string fileName, string contentType, Stream fileStream, CancellationToken cancellationToken)
    {
        var settings = await _store.GetDriveSettingsAsync(cancellationToken);
        return await _driveService.UploadFileAsync(settings, folderId, fileName, contentType, fileStream, cancellationToken);
    }

    public async Task<(Stream Content, string ContentType, string FileName, long Size)?> DownloadDriveFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var settings = await _store.GetDriveSettingsAsync(cancellationToken);
        return await _driveService.DownloadFileAsync(settings, fileId, cancellationToken);
    }

    public async Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken)
    {
        var bootstrap = await _store.GetBootstrapAsync(cancellationToken);
        
        try
        {
            var quota = await _driveService.GetStorageQuotaAsync(bootstrap.Drive, cancellationToken);
            var updatedDrive = bootstrap.Drive with 
            { 
                StorageUsed = quota.Used, 
                StorageAvailable = quota.Limit 
            };
            return bootstrap with { Drive = updatedDrive };
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Bootstrap Drive Quota Error: {ex.Message}");
            var updatedDrive = bootstrap.Drive with { StorageQuotaError = ex.Message };
            return bootstrap with { Drive = updatedDrive };
        }
    }

    public Task<DashboardWorkspaceDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
        => _store.GetDashboardSummaryAsync(cancellationToken);

    public Task<PaginatedListDto<VideoItemDto>> GetVideosByStageAsync(string stage, int page, int pageSize, CancellationToken cancellationToken)
        => _store.GetVideosByStageAsync(stage, page, pageSize, cancellationToken);

    public Task<PaginatedListDto<AgentRunDto>> GetAgentRunsAsync(int page, int pageSize, CancellationToken cancellationToken)
        => _store.GetAgentRunsAsync(page, pageSize, cancellationToken);

    public async Task<ConnectionTestResult> TestAgentConnectionAsync(string agentKey, CancellationToken cancellationToken)
    {
        var settings = await _store.GetAgentSettingsAsync(agentKey, cancellationToken);
        if (settings == null) return new ConnectionTestResult(false, "Agent not found.");

        var provider = _chatProviders.FirstOrDefault(p => string.Equals(p.ProviderName, settings.UseOpenRouter ? "OpenRouter" : settings.ProviderName, StringComparison.OrdinalIgnoreCase));
        if (provider == null) return new ConnectionTestResult(false, $"Provider '{settings.ProviderName}' not supported for chat.");

        try
        {
            var apiKey = settings.UseOpenRouter ? settings.OpenRouterApiKey : settings.ApiKey;
            var model = settings.UseOpenRouter ? settings.OpenRouterModel : settings.ModelName;

            if (string.IsNullOrWhiteSpace(apiKey)) return new ConnectionTestResult(false, "API Key is missing.");

            var result = await provider.CompleteAsync(
                new ChatCompletionRequest("You are a connection tester. Reply with 'OK'.", [new ChatMessageInput("user", "ping")], model),
                apiKey,
                settings.BaseUrl,
                cancellationToken);

            return new ConnectionTestResult(true, "Successfully connected to provider.", result.Content);
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult(false, "Connection failed.", ex.Message);
        }
    }

    public async Task<ConnectionTestResult> TestDriveConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _store.GetDriveSettingsAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(settings.ClientId)) return new ConnectionTestResult(false, "Google Client ID is missing.");

            var files = await _driveService.ListFilesAsync(settings, null, cancellationToken);
            return new ConnectionTestResult(true, $"Successfully connected. Found {files.Count} files in root folder.");
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult(false, "Drive connection failed.", ex.Message);
        }
    }


    public async Task<AgentChatResponse> SendAgentMessageAsync(
        string agentKey,
        SendAgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new AgentChatResponse(true, "Send a message before starting the chat.", []);
        }

        var context = await _store.GetAgentContextAsync(agentKey, cancellationToken);
        if (context is null)
        {
            return new AgentChatResponse(true, "This agent is not available yet.", []);
        }

        var settings = await _store.GetAgentSettingsAsync(agentKey, cancellationToken);
        if (!await _store.IsAgentWithinBudgetAsync(agentKey, cancellationToken))
        {
            return new AgentChatResponse(true, $"[Budget Exceeded] {context.Agent.Name} has hit its token or cost limit. Increase the budget in Settings to continue live execution.", []);
        }

        var rateLimitKey = $"ratelimit_{agentKey}";
        var requestCount = _cache.GetOrCreate(rateLimitKey, entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });
        if (requestCount >= 10) return new AgentChatResponse(true, $"[Rate Limited] {context.Agent.Name} is making too many requests. Please wait a minute.", []);
        _cache.Set(rateLimitKey, requestCount + 1, TimeSpan.FromMinutes(1));

        var assistantReply = string.Empty;
        int tokensIn = 0, tokensOut = 0, durationMs = 0;
        decimal cost = 0;

        var effectiveProviderName = settings?.UseOpenRouter == true ? "OpenRouter" : context.Agent.ProviderName;
        var provider = _chatProviders.FirstOrDefault(p => string.Equals(p.ProviderName, effectiveProviderName, StringComparison.OrdinalIgnoreCase));

        if (provider != null && context.Agent.IsConnected && settings != null)
        {
            try
            {
                var systemPrompt = BuildSystemPrompt(context);
                var history = context.Messages
                    .Where(m => !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith("No response content generated"))
                    .Select(m => new ChatMessageInput(m.Role, m.Content))
                    .ToList();
                history.Add(new ChatMessageInput("user", request.Message));
                
                var apiKey = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterApiKey) ? settings.OpenRouterApiKey : settings.ApiKey;
                var model = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterModel) ? settings.OpenRouterModel : settings.ModelName;
                    
                var result = await provider.CompleteAsync(new ChatCompletionRequest(systemPrompt, history, model), apiKey, settings.BaseUrl, cancellationToken);
                assistantReply = result.Content;
                tokensIn = result.TokensIn;
                tokensOut = result.TokensOut;
                cost = result.CostUsd;
                durationMs = result.DurationMs;
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

        var messages = await _store.SaveAgentExchangeAsync(agentKey, request.Message, assistantReply, tokensIn, tokensOut, cost, durationMs, cancellationToken);

        return new AgentChatResponse(
            false,
            context.Agent.RequiresConnection && !context.Agent.IsConnected
                ? $"[SIMULATION] {context.Agent.Name} responded in workspace-preview mode. Save credentials in Settings when you are ready for live execution."
                : $"{context.Agent.Name} responded successfully.",
            messages);
    }

    public async IAsyncEnumerable<AgentStreamChunk> StreamAgentMessageAsync(
        string agentKey,
        SendAgentMessageRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            yield return new AgentStreamChunk("delta", "Send a message before starting the chat.");
            yield break;
        }

        var context = await _store.GetAgentContextAsync(agentKey, cancellationToken);
        if (context is null)
        {
            yield return new AgentStreamChunk("delta", "This agent is not available yet.");
            yield break;
        }

        var settings = await _store.GetAgentSettingsAsync(agentKey, cancellationToken);
        if (!await _store.IsAgentWithinBudgetAsync(agentKey, cancellationToken))
        {
            yield return new AgentStreamChunk("delta", $"[Budget Exceeded] {context.Agent.Name} has hit its token or cost limit. Increase the budget in Settings to continue live execution.");
            yield break;
        }
        var effectiveProviderName = settings?.UseOpenRouter == true ? "OpenRouter" : context.Agent.ProviderName;
        var provider = _chatProviders.FirstOrDefault(p => string.Equals(p.ProviderName, effectiveProviderName, StringComparison.OrdinalIgnoreCase));

        var fullResponse = new StringBuilder();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var history = context.Messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith("No response content generated"))
            .Select(m => new ChatMessageInput(m.Role, m.Content))
            .ToList();
        history.Add(new ChatMessageInput("user", request.Message));

        if (provider != null && context.Agent.IsConnected && settings != null)
        {
            var systemPrompt = BuildSystemPrompt(context);
            var apiKey = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterApiKey) ? settings.OpenRouterApiKey : settings.ApiKey;
            var model = settings.UseOpenRouter && !string.IsNullOrWhiteSpace(settings.OpenRouterModel) ? settings.OpenRouterModel : settings.ModelName;

            IAsyncEnumerable<string>? stream = null;
            string? streamError = null;
            try {
                stream = provider.StreamAsync(new ChatCompletionRequest(systemPrompt, history, model), apiKey, settings.BaseUrl, cancellationToken);
            } catch (Exception ex) {
                streamError = $"[Stream Error] {ex.Message}. Falling back to simulation...";
            }

            if (streamError != null)
            {
                fullResponse.Append(streamError);
                yield return new AgentStreamChunk("delta", streamError);
            }

            if (stream != null)
            {
                await foreach (var delta in stream)
                {
                    fullResponse.Append(delta);
                    yield return new AgentStreamChunk("delta", delta);
                }
            }
        }

        if (fullResponse.Length == 0)
        {
            var reply = BuildAgentReply(request.Message, context);
            var chunks = reply.Split(' ');
            foreach (var chunk in chunks)
            {
                var delta = chunk + " ";
                fullResponse.Append(delta);
                yield return new AgentStreamChunk("delta", delta);
                await Task.Delay(30, cancellationToken);
            }
        }

        // For streaming, we record usage at the end. 
        // Note: tokensIn/Out are harder to get precisely for streaming without a custom parser, 
        // using approximations based on character count for now if the provider doesn't return them in stream metadata.
        int streamTokensIn = history.Sum(h => h.Content.Length) / 3;
        int streamTokensOut = fullResponse.Length / 2;
        decimal streamCost = (streamTokensIn + streamTokensOut) * 0.00001m;

        var messages = await _store.SaveAgentExchangeAsync(agentKey, request.Message, fullResponse.ToString(), streamTokensIn, streamTokensOut, streamCost, (int)sw.ElapsedMilliseconds, cancellationToken);
        yield return new AgentStreamChunk("done", "Completed", messages.LastOrDefault());
    }

    public Task<MemoryRecordDto?> ApproveMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken)
        => _store.ReviewMemoryAsync(id, "Approved", request, cancellationToken);

    public Task<MemoryRecordDto?> RejectMemoryAsync(Guid id, ReviewMemoryRequest request, CancellationToken cancellationToken)
        => _store.ReviewMemoryAsync(id, "Rejected", request, cancellationToken);

    public Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken)
        => _store.UpdateVideoStageAsync(id, request, cancellationToken);

    public Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken)
        => _store.CreateManualScheduleAsync(request, cancellationToken);

    public Task<AgentSettingsDto?> SaveAgentSettingsAsync(string agentKey, SaveAgentSettingsRequest request, CancellationToken cancellationToken)
        => _store.SaveAgentSettingsAsync(agentKey, request, cancellationToken);

    public Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken)
        => _store.GetPendingMemorySuggestionsAsync(cancellationToken);

    public Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken)
        => _store.SaveDriveSettingsAsync(request, cancellationToken);

    public async Task<string> RegisterDriveWebhookAsync(string folderId, string webhookUrl, CancellationToken cancellationToken)
    {
        var settings = await _store.GetDriveSettingsAsync(cancellationToken);
        return await _driveService.WatchFolderAsync(settings, folderId, webhookUrl, cancellationToken);
    }

    public Task<VideoItemDto?> LinkVideoToAssetAsync(Guid id, string driveFileId, CancellationToken cancellationToken)
        => _store.LinkVideoToAssetAsync(id, driveFileId, cancellationToken);

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

    private string BuildSystemPrompt(AgentConversationContextDto context)
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

        builder.AppendLine();
        builder.AppendLine("=== Available Tools ===");
        foreach (var tool in _toolRegistry.GetTools())
        {
            builder.AppendLine($"- {tool.Name}: {tool.Description}");
            builder.AppendLine($"  Usage: Reply with [TOOL_CALL:{tool.Name} {{\"arg\":\"val\"}}] to execute.");
        }

        return builder.ToString();
    }
}
