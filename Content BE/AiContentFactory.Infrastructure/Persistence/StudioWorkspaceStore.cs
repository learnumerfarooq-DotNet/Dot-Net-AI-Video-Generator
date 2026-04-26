 using AiContentFactory.Application.Studio;
 using AiContentFactory.Application.ContentFactory;
 using AiContentFactory.Domain.Memory;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Persistence;

public sealed class StudioWorkspaceStore(
    StudioDbContext dbContext, 
    IMemoryRepository memoryRepository,
    IJsonFileStore jsonStore) : IStudioWorkspaceStore
{
    private static readonly string[] Palette =
    [
        "#1769aa",
        "#0f9d58",
        "#ef6c00",
        "#7b61ff",
        "#b83f6b",
        "#00838f",
        "#5d4037",
        "#607d8b",
        "#6a1b9a",
        "#2e7d32",
        "#ad1457"
    ];

    public async Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken)
    {
        var agents = await dbContext.Agents
            .OrderBy(agent => agent.SortOrder)
            .ToListAsync(cancellationToken);

        var usages = await dbContext.AgentUsages
            .OrderBy(usage => usage.CapturedAt)
            .ToListAsync(cancellationToken);

        var memories = await dbContext.Memories
            .OrderByDescending(memory => memory.UpdatedAt)
            .ToListAsync(cancellationToken);

        var videos = await dbContext.Videos
            .OrderByDescending(video => video.CreatedAt)
            .ToListAsync(cancellationToken);

        var publications = await dbContext.Publications
            .ToListAsync(cancellationToken);

        var schedules = await dbContext.ScheduleJobs
            .OrderBy(job => job.Name)
            .ToListAsync(cancellationToken);

        var runs = await dbContext.AgentRuns
            .OrderByDescending(run => run.QueuedAt)
            .ToListAsync(cancellationToken);

        var chatMessages = await dbContext.ChatMessages
            .OrderByDescending(message => message.CreatedAt)
            .Take(200)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync(cancellationToken);

        var recentRunsByAgent = runs
            .GroupBy(run => run.AgentKey)
            .ToDictionary(group => group.Key, group => group.Take(2).Select(ToRunDto).ToArray());

        var localHighlightsByAgent = memories
            .Where(memory => memory.Scope == "Local" && memory.Status == "Approved" && !string.IsNullOrWhiteSpace(memory.AgentKey))
            .GroupBy(memory => memory.AgentKey!)
            .ToDictionary(group => group.Key, group => group.Take(2).Select(memory => memory.Title).ToArray());

        var agentDtos = agents
            .Select(agent => ToAgentSummaryDto(
                agent,
                localHighlightsByAgent.GetValueOrDefault(agent.Key) ?? [],
                recentRunsByAgent.GetValueOrDefault(agent.Key) ?? []))
            .ToArray();

        var readyVideos = videos.Where(video => video.Stage == "ReadyToUpload").Select(ToVideoDto).ToArray();
        var backlogVideos = videos.Where(video => video.Stage == "Backlog").Select(ToVideoDto).ToArray();
        var publishedVideos = videos.Where(video => video.Stage == "Published").Select(ToVideoDto).ToArray();

        var driveSettings = await GetDriveSettingsAsync(cancellationToken);

        return new WorkspaceBootstrapResponse(
            new DashboardWorkspaceDto(
                BuildUsageSeries(agents, usages),
                new MemoryCountsDto(
                    memories.Count(memory => memory.Scope == "Global" && memory.Status == "Approved"),
                    memories.Count(memory => memory.Scope == "Local" && memory.Status == "Approved"),
                    memories.Count(memory => memory.Status == "Pending")),
                readyVideos,
                backlogVideos,
                BuildPublicationWidgets(publications),
                publishedVideos.Take(6).ToArray(),
                runs.Take(8).Select(ToRunDto).ToArray()),
            new AgentWorkspaceDto(
                agentDtos,
                chatMessages.Select(ToChatDto).ToArray()),
            new MemoryWorkspaceDto(
                new MemoryCountsDto(
                    memories.Count(memory => memory.Scope == "Global" && memory.Status == "Approved"),
                    memories.Count(memory => memory.Scope == "Local" && memory.Status == "Approved"),
                    memories.Count(memory => memory.Status == "Pending")),
                memories.Where(memory => memory.Status == "Pending").Select(ToMemoryDto).ToArray(),
                memories.Where(memory => memory.Scope == "Global" && memory.Status == "Approved").Select(ToMemoryDto).ToArray(),
                memories.Where(memory => memory.Scope == "Local" && memory.Status == "Approved").Select(ToMemoryDto).ToArray()),
            new SchedulerWorkspaceDto(
                schedules.Where(job => job.Type == "Manual").Select(ToScheduleDto).ToArray(),
                schedules.Where(job => job.Type == "DailyPosting").Select(ToScheduleDto).ToArray(),
                schedules.Where(job => job.Type == "RetryUploads").Select(ToScheduleDto).ToArray(),
                schedules.Where(job => job.Type == "QueueExecution").Select(ToScheduleDto).ToArray()),
            new SettingsWorkspaceDto(
                agents.Select(ToSettingsDto).ToArray(),
                BuildProviderOptions()),
            driveSettings,
            DateTimeOffset.UtcNow);
    }

    public async Task<AgentConversationContextDto?> GetAgentContextAsync(string agentKey, CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(item => item.Key == agentKey, cancellationToken);
        if (agent is null)
        {
            return null;
        }

        var messages = await dbContext.ChatMessages
            .Where(message => message.AgentKey == agentKey)
            .OrderByDescending(message => message.CreatedAt)
            .Take(20)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync(cancellationToken);

        var memories = await dbContext.Memories
            .Where(memory => memory.Status == "Approved")
            .OrderByDescending(memory => memory.UpdatedAt)
            .ToListAsync(cancellationToken);

        var videos = await dbContext.Videos
            .OrderByDescending(video => video.CreatedAt)
            .ToListAsync(cancellationToken);

        var runs = await dbContext.AgentRuns
            .Where(run => run.AgentKey == agentKey)
            .OrderByDescending(run => run.QueuedAt)
            .Take(2)
            .ToListAsync(cancellationToken);

        return new AgentConversationContextDto(
            ToAgentSummaryDto(agent,
                memories.Where(memory => memory.Scope == "Local" && memory.AgentKey == agentKey).Select(memory => memory.Title).Take(2).ToArray(),
                runs.Select(ToRunDto).ToArray()),
            messages.Select(ToChatDto).ToArray(),
            memories.Where(memory => memory.Scope == "Global").Take(4).Select(ToMemoryDto).ToArray(),
            memories.Where(memory => memory.Scope == "Local" && memory.AgentKey == agentKey).Take(4).Select(ToMemoryDto).ToArray(),
            videos.Where(video => video.Stage == "Backlog").Take(4).Select(ToVideoDto).ToArray(),
            videos.Where(video => video.Stage == "ReadyToUpload").Take(4).Select(ToVideoDto).ToArray());
    }

    public async Task<IReadOnlyList<ChatMessageDto>> SaveAgentExchangeAsync(
        string agentKey,
        string userMessage,
        string assistantMessage,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        dbContext.ChatMessages.Add(new StudioChatMessageEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            Role = "user",
            Content = userMessage,
            CreatedAt = now
        });

        dbContext.ChatMessages.Add(new StudioChatMessageEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            Role = "assistant",
            Content = assistantMessage,
            CreatedAt = now.AddSeconds(1)
        });

        dbContext.AgentUsages.Add(new StudioAgentUsageEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            CapturedAt = now,
            RequestCount = 1,
            TokensIn = Math.Max(150, userMessage.Length * 3),
            TokensOut = Math.Max(240, assistantMessage.Length * 2),
            CostUsd = 0.19m,
            DurationMs = 1200
        });

        dbContext.AgentRuns.Add(new StudioAgentRunEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            Title = "Interactive chat guidance",
            Status = "Succeeded",
            Summary = "Agent reviewed workspace context and returned a conversation response.",
            QueuedAt = now,
            CompletedAt = now.AddSeconds(2)
        });

        var agent = await dbContext.Agents.FirstOrDefaultAsync(item => item.Key == agentKey, cancellationToken);
        if (agent is not null)
        {
            agent.LastRunAt = now;
            agent.UpdatedAt = now;
            agent.Status = agent.IsConnected ? "Connected" : "Connect API first";
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.ChatMessages
            .Where(message => message.AgentKey == agentKey)
            .OrderByDescending(message => message.CreatedAt)
            .Take(20)
            .OrderBy(message => message.CreatedAt)
            .Select(message => ToChatDto(message))
            .ToListAsync(cancellationToken);
    }

    public async Task<MemoryRecordDto?> ReviewMemoryAsync(
        Guid id,
        string status,
        ReviewMemoryRequest request,
        CancellationToken cancellationToken)
    {
        var memory = await dbContext.Memories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (memory is null)
        {
            return null;
        }

        memory.Title = string.IsNullOrWhiteSpace(request.RevisedTitle) ? memory.Title : request.RevisedTitle;
        memory.Content = string.IsNullOrWhiteSpace(request.RevisedContent) ? memory.Content : request.RevisedContent;
        memory.Status = status;
        memory.UpdatedAt = DateTimeOffset.UtcNow;
        memory.ApprovedAt = status == "Approved" ? DateTimeOffset.UtcNow : null;
        
        // Self-Improving System: generate a memory improvement suggestion when a memory is approved
        if (status == "Approved" && memory != null)
        {
            var now = DateTimeOffset.UtcNow;
            var preview = memory.Content != null && memory.Content.Length > 256 ? memory.Content.Substring(0, 256) : memory.Content ?? string.Empty;
            var improvementContent = $"Auto-improvement for memory '{memory.Title}': {preview}...";
            var scopeEnum = memory.Scope == "Global"
                ? AiContentFactory.Domain.Memory.MemoryScope.Global
                : AiContentFactory.Domain.Memory.MemoryScope.Local;
            var improvement = new AiContentFactory.Domain.Memory.MemorySuggestion(
                Guid.NewGuid(),
                scopeEnum,
                memory.AgentKey,
                improvementContent,
                "Auto-improvement suggestion generated on approval",
                AiContentFactory.Domain.Memory.MemorySuggestionStatus.Pending,
                now
            );
            await memoryRepository.SuggestAsync(improvement, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToMemoryDto(memory);
    }

    public async Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken)
    {
        var suggestions = await memoryRepository.GetPendingSuggestionsAsync(cancellationToken);
        return suggestions.Select(ToMemorySuggestionDto).ToArray();
    }

    // ToMemorySuggestionDto moved to the later block for a single definition

    public async Task<VideoItemDto?> UpdateVideoStageAsync(
        Guid id,
        UpdateVideoStageRequest request,
        CancellationToken cancellationToken)
    {
        var video = await dbContext.Videos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (video is null)
        {
            return null;
        }

        video.Stage = request.Stage;
        video.UpdatedAt = DateTimeOffset.UtcNow;
        video.PublishedAt = request.Stage == "Published" ? DateTimeOffset.UtcNow : video.PublishedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToVideoDto(video);
    }

    public async Task<ScheduleJobDto> CreateManualScheduleAsync(
        CreateManualScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var job = new StudioScheduleJobEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = "Manual",
            AgentKey = request.AgentKey,
            IsEnabled = request.IsEnabled,
            Status = request.IsEnabled ? "Queued" : "Disabled",
            Trigger = request.Trigger,
            QueueMode = "Manual",
            NextRunAt = request.IsEnabled ? DateTimeOffset.UtcNow.AddHours(2) : null,
            LastRunAt = null,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ScheduleJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToScheduleDto(job);
    }

    public async Task<AgentSettingsDto?> SaveAgentSettingsAsync(
        string agentKey,
        SaveAgentSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(item => item.Key == agentKey, cancellationToken);
        if (agent is null)
        {
            return null;
        }

        agent.ProviderName = request.ProviderName.Trim();
        agent.ModelName = request.ModelName.Trim();
        agent.BaseUrl = request.BaseUrl.Trim();
        agent.ApiKey = request.ApiKey.Trim();
            agent.ClientId = request.ClientId.Trim();
            agent.ClientSecret = request.ClientSecret.Trim();
        agent.RefreshToken = request.RefreshToken.Trim();
        agent.SourceVideoPath = request.SourceVideoPath.Trim();
        agent.StorageFolderId = request.StorageFolderId.Trim();
        agent.StorageFolderName = request.StorageFolderName.Trim();
        agent.StorageFolderPath = request.StorageFolderPath.Trim();
        agent.StorageFolderUrl = request.StorageFolderUrl.Trim();
        agent.UseOpenRouter = request.UseOpenRouter && agent.SupportsOpenRouter;
        agent.OpenRouterModel = request.OpenRouterModel.Trim();
        agent.OpenRouterApiKey = request.OpenRouterApiKey.Trim();
        agent.Notes = request.Notes.Trim();
        agent.IsConnected = DetermineConnection(agent);
        agent.Status = agent.IsConnected ? "Connected" : "Connect API first";
        agent.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSettingsDto(agent);
    }

    public async Task<AgentSettingsDto?> GetAgentSettingsAsync(string agentKey, CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(item => item.Key == agentKey, cancellationToken);
        return agent is null ? null : ToSettingsDto(agent);
    }

    public async Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken)
    {
        var settings = new DriveSettingsDto(request.ClientId, request.ClientSecret, request.RefreshToken, request.RootFolderId);
        await jsonStore.WriteAsync("drive.config.json", settings, cancellationToken);
        return settings;
    }

    public Task<DriveSettingsDto> GetDriveSettingsAsync(CancellationToken cancellationToken)
        => jsonStore.ReadAsync("drive.config.json", new DriveSettingsDto("", "", "", ""), cancellationToken);

    private static bool DetermineConnection(StudioAgentEntity agent)
    {
        if (!agent.RequiresConnection)
        {
            return true;
        }

        if (agent.UseOpenRouter && !string.IsNullOrWhiteSpace(agent.OpenRouterApiKey))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(agent.ApiKey))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(agent.ClientId) && !string.IsNullOrWhiteSpace(agent.ClientSecret))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(agent.RefreshToken);
    }

    private static UsageSeriesDto[] BuildUsageSeries(
        IReadOnlyList<StudioAgentEntity> agents,
        IReadOnlyList<StudioAgentUsageEntity> usages)
    {
        return agents.Select((agent, index) =>
            new UsageSeriesDto(
                agent.Key,
                agent.Name,
                Palette[index % Palette.Length],
                usages.Where(usage => usage.AgentKey == agent.Key)
                    .OrderBy(usage => usage.CapturedAt)
                    .TakeLast(7)
                    .Select(usage => new UsagePointDto(
                        usage.CapturedAt,
                        usage.RequestCount,
                        usage.TokensIn,
                        usage.TokensOut,
                        usage.CostUsd,
                        usage.DurationMs))
                    .ToArray()))
            .ToArray();
    }

    private static PlatformPublicationWidgetDto[] BuildPublicationWidgets(IReadOnlyList<StudioPublicationEntity> publications)
    {
        return publications
            .GroupBy(publication => publication.Platform)
            .OrderBy(group => group.Key)
            .Select(group => new PlatformPublicationWidgetDto(
                group.Key,
                group.Count(item => item.Status == "Published"),
                group.Count(item => item.Status == "Scheduled"),
                group.Count(item => item.Status == "Failed"),
                group.Where(item => item.Status == "Published").Sum(item => item.Views)))
            .ToArray();
    }

    private static ProviderOptionDto[] BuildProviderOptions()
    {
        return
        [
            new ProviderOptionDto("Brain", ["OpenAI", "Claude", "Gemini", "OpenRouter"]),
            new ProviderOptionDto("Discovery", ["OpenAI", "Gemini", "OpenRouter"]),
            new ProviderOptionDto("Writing", ["OpenAI", "Claude", "Gemini", "OpenRouter"]),
            new ProviderOptionDto("Video", ["Runway", "Pika", "Luma", "Manual"]),
            new ProviderOptionDto("Shorts", ["OpenAI", "Gemini", "OpenRouter"]),
            new ProviderOptionDto("Publishing", ["YouTube", "TikTok", "Instagram", "Facebook", "LinkedIn", "DryRun"])
        ];
    }

    private static AgentSummaryDto ToAgentSummaryDto(
        StudioAgentEntity agent,
        IReadOnlyList<string> localMemoryHighlights,
        IReadOnlyList<AgentRunDto> recentRuns)
    {
        return new AgentSummaryDto(
            agent.Key,
            agent.Name,
            agent.Description,
            agent.Category,
            agent.RequiresConnection,
            agent.SupportsOpenRouter,
            agent.IsConnected,
            agent.ProviderName,
            agent.ModelName,
            agent.Status,
            agent.CapabilitySummary,
            agent.LastRunAt,
            localMemoryHighlights,
            recentRuns);
    }

    private static AgentSettingsDto ToSettingsDto(StudioAgentEntity agent)
    {
        return new AgentSettingsDto(
            agent.Key,
            agent.Name,
            agent.Category,
            agent.RequiresConnection,
            agent.SupportsOpenRouter,
            agent.IsConnected,
            agent.ProviderName,
            agent.ModelName,
            agent.BaseUrl,
            agent.ApiKey,
            agent.ClientId,
            agent.ClientSecret,
            agent.RefreshToken,
            agent.SourceVideoPath,
            agent.StorageFolderId,
            agent.StorageFolderName,
            agent.StorageFolderPath,
            agent.StorageFolderUrl,
            agent.UseOpenRouter,
            agent.OpenRouterModel,
            agent.OpenRouterApiKey,
            agent.Notes,
            agent.UpdatedAt);
    }

    private static MemoryRecordDto ToMemoryDto(StudioMemoryEntity memory)
    {
        return new MemoryRecordDto(
            memory.Id,
            memory.Scope,
            memory.AgentKey,
            memory.Title,
            memory.Content,
            memory.Status,
            memory.Tags,
            memory.CreatedAt,
            memory.UpdatedAt,
            memory.ApprovedAt);
    }

    private static VideoItemDto ToVideoDto(StudioVideoEntity video)
    {
        return new VideoItemDto(
            video.Id,
            video.Title,
            video.Topic,
            video.Format,
            video.Stage,
            video.StorageFolder,
            video.DriveFileId,
            video.SourceAgentKey,
            video.Platforms,
            video.CreatedAt,
            video.PublishedAt);
    }

    private static MemorySuggestionDto ToMemorySuggestionDto(MemorySuggestion s)
    {
        return new MemorySuggestionDto(
            s.Id,
            s.Scope.ToString(),
            s.AgentName,
            s.Content,
            s.Reason,
            s.Status.ToString(),
            s.CreatedAt);
    }

    private static ScheduleJobDto ToScheduleDto(StudioScheduleJobEntity job)
    {
        return new ScheduleJobDto(
            job.Id,
            job.Name,
            job.Type,
            job.AgentKey,
            job.IsEnabled,
            job.Status,
            job.Trigger,
            job.QueueMode,
            job.NextRunAt,
            job.LastRunAt,
            job.Notes);
    }

    private static ChatMessageDto ToChatDto(StudioChatMessageEntity message)
    {
        return new ChatMessageDto(
            message.Id,
            message.AgentKey,
            message.Role,
            message.Content,
            message.CreatedAt);
    }

    private static AgentRunDto ToRunDto(StudioAgentRunEntity run)
    {
        return new AgentRunDto(
            run.Id,
            run.AgentKey,
            run.Title,
            run.Status,
            run.Summary,
            run.QueuedAt,
            run.CompletedAt);
    }
}
