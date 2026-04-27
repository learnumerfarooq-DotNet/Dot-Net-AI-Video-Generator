using AiContentFactory.Application.Common;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Events;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace AiContentFactory.Infrastructure.Persistence;

public sealed class StudioWorkspaceStore : IStudioWorkspaceStore
{
    private readonly StudioDbContext dbContext;
    private readonly IMemoryRepository memoryRepository;
    private readonly IJsonFileStore jsonStore;
    private readonly IMemoryCache cache;
    private readonly IRealtimeEventEmitter emitter;
    private readonly ISchedulerFactory schedulerFactory;
    private readonly IEncryptionService encryption;
    private readonly IMaskingService masking;

    public StudioWorkspaceStore(
        StudioDbContext dbContext,
        IMemoryRepository memoryRepository,
        IJsonFileStore jsonStore,
        IMemoryCache cache,
        IRealtimeEventEmitter emitter,
        ISchedulerFactory schedulerFactory,
        IEncryptionService encryption,
        IMaskingService masking)
    {
        this.dbContext = dbContext;
        this.memoryRepository = memoryRepository;
        this.jsonStore = jsonStore;
        this.cache = cache;
        this.emitter = emitter;
        this.schedulerFactory = schedulerFactory;
        this.encryption = encryption;
        this.masking = masking;
    }
    private static readonly string[] Palette =
    [
        "#1769aa", "#0f9d58", "#ef6c00", "#7b61ff", "#b83f6b", 
        "#00838f", "#5d4037", "#607d8b", "#6a1b9a", "#2e7d32", "#ad1457"
    ];

    public async Task<WorkspaceBootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[DEBUG] GetBootstrapAsync started");
        
        Console.WriteLine("[DEBUG] Fetching agents...");
        var agents = await dbContext.Agents.AsNoTracking().OrderBy(a => a.SortOrder).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching usages...");
        var usages = await dbContext.AgentUsages.AsNoTracking().OrderBy(u => u.CapturedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching global memories...");
        var globalMemories = await dbContext.GlobalMemories.AsNoTracking().OrderByDescending(m => m.UpdatedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching agent memories...");
        var agentMemories = await dbContext.AgentMemories.AsNoTracking().OrderByDescending(m => m.UpdatedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching videos...");
        var videos = await dbContext.Videos.AsNoTracking().OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching publications...");
        var publications = await dbContext.Publications.AsNoTracking().ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching schedules...");
        var schedules = await dbContext.ScheduleJobs.AsNoTracking().OrderBy(j => j.Name).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching runs...");
        var runs = await dbContext.AgentRuns.AsNoTracking().OrderByDescending(r => r.QueuedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching chat messages...");
        var chatMessages = await dbContext.ChatMessages.AsNoTracking().OrderByDescending(m => m.CreatedAt).Take(200).OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching connections...");
        var connections = await dbContext.AgentConnections.AsNoTracking().ToDictionaryAsync(c => c.AgentKey, cancellationToken);
        
        Console.WriteLine("[DEBUG] Fetching drive settings...");
        var driveSettings = await GetDriveSettingsAsync(cancellationToken);
        
        Console.WriteLine("[DEBUG] All data fetched. Starting mapping...");

        // Efficient lookup maps
        var recentRunsByAgent = runs.GroupBy(r => r.AgentKey).ToDictionary(g => g.Key, g => g.Take(2).Select(ToRunDto).ToArray());
        var localHighlightsByAgent = agentMemories.Where(m => m.Status == "Approved").GroupBy(m => m.AgentKey).ToDictionary(g => g.Key, g => g.Take(2).Select(m => m.Title).ToArray());

        var agentDtos = agents.Select(a => ToAgentSummaryDto(a, connections.GetValueOrDefault(a.Key), localHighlightsByAgent.GetValueOrDefault(a.Key) ?? [], recentRunsByAgent.GetValueOrDefault(a.Key) ?? [])).ToArray();

        var readyVideos = videos.Where(v => v.Stage == "ReadyToUpload").Select(ToVideoDto).ToArray();
        var backlogVideos = videos.Where(v => v.Stage == "Backlog").Select(ToVideoDto).ToArray();
        var publishedVideos = videos.Where(v => v.Stage == "Published").Select(ToVideoDto).ToArray();

        return new WorkspaceBootstrapResponse(
            new DashboardWorkspaceDto(
                BuildUsageSeries(agents, usages),
                new MemoryCountsDto(
                    globalMemories.Count(m => m.Status == "Approved"),
                    agentMemories.Count(m => m.Status == "Approved"),
                    globalMemories.Count(m => m.Status == "Pending") + agentMemories.Count(m => m.Status == "Pending")),
                readyVideos.Take(5).ToArray(),
                backlogVideos.Take(5).ToArray(),
                BuildPublicationWidgets(publications),
                publishedVideos.Take(6).ToArray(),
                runs.Take(8).Select(ToRunDto).ToArray()),
            new AgentWorkspaceDto(agentDtos, chatMessages.Select(ToChatDto).ToArray()),
            new MemoryWorkspaceDto(
                new MemoryCountsDto(
                    globalMemories.Count(m => m.Status == "Approved"),
                    agentMemories.Count(m => m.Status == "Approved"),
                    globalMemories.Count(m => m.Status == "Pending") + agentMemories.Count(m => m.Status == "Pending")),
                globalMemories.Where(m => m.Status == "Pending").Select(ToMemoryDto).Concat(agentMemories.Where(m => m.Status == "Pending").Select(ToMemoryDto)).ToArray(),
                globalMemories.Where(m => m.Status == "Approved").Select(ToMemoryDto).ToArray(),
                agentMemories.Where(m => m.Status == "Approved").Select(ToMemoryDto).ToArray()),
            new SchedulerWorkspaceDto(
                schedules.Where(j => j.Type == "Manual").Select(ToScheduleJobDto).ToArray(),
                schedules.Where(j => j.Type == "DailyPosting").Select(ToScheduleJobDto).ToArray(),
                schedules.Where(j => j.Type == "RetryUploads").Select(ToScheduleJobDto).ToArray(),
                schedules.Where(j => j.Type == "QueueExecution").Select(ToScheduleJobDto).ToArray()),
            new SettingsWorkspaceDto(agents.Select(a => ToSettingsDto(a, connections.GetValueOrDefault(a.Key))).ToArray(), BuildProviderOptions()),
            driveSettings,
            DateTimeOffset.UtcNow);
    }


    public async Task<DashboardWorkspaceDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "dashboard_summary";
        if (cache.TryGetValue(cacheKey, out DashboardWorkspaceDto? cachedSummary)) return cachedSummary!;

        var agents = await dbContext.Agents.OrderBy(a => a.SortOrder).ToListAsync(cancellationToken);
        var usages = await dbContext.AgentUsages.OrderBy(u => u.CapturedAt).ToListAsync(cancellationToken);
        var globalMemories = await dbContext.GlobalMemories.ToListAsync(cancellationToken);
        var agentMemories = await dbContext.AgentMemories.ToListAsync(cancellationToken);
        var publications = await dbContext.Publications.ToListAsync(cancellationToken);
        var runs = await dbContext.AgentRuns.OrderByDescending(r => r.QueuedAt).Take(8).ToListAsync(cancellationToken);
        var videos = await dbContext.Videos.ToListAsync(cancellationToken);

        var connections = await dbContext.AgentConnections.ToDictionaryAsync(c => c.AgentKey, cancellationToken);

        var summary = new DashboardWorkspaceDto(
            BuildUsageSeries(agents, usages),
            new MemoryCountsDto(
                globalMemories.Count(m => m.Status == "Approved"),
                agentMemories.Count(m => m.Status == "Approved"),
                globalMemories.Count(m => m.Status == "Pending") + agentMemories.Count(m => m.Status == "Pending")),
            videos.Where(v => v.Stage == "ReadyToUpload").Take(5).Select(ToVideoDto).ToArray(),
            videos.Where(v => v.Stage == "Backlog").Take(5).Select(ToVideoDto).ToArray(),
            BuildPublicationWidgets(publications),
            videos.Where(v => v.Stage == "Published").Take(6).Select(ToVideoDto).ToArray(),
            runs.Select(ToRunDto).ToArray());

        cache.Set(cacheKey, summary, TimeSpan.FromMinutes(5));
        return summary;
    }

    public async Task<PaginatedListDto<VideoItemDto>> GetVideosByStageAsync(string stage, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Videos.Where(v => v.Stage == stage);
        var total = await query.CountAsync(cancellationToken);
        
        var videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedListDto<VideoItemDto>(
            videos.Select(ToVideoDto).ToArray(),
            total,
            page,
            pageSize);
    }

    public async Task<PaginatedListDto<AgentRunDto>> GetAgentRunsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AgentRuns.AsQueryable();
        var total = await query.CountAsync(cancellationToken);

        var runs = await query
            .OrderByDescending(r => r.QueuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedListDto<AgentRunDto>(
            runs.Select(ToRunDto).ToArray(),
            total,
            page,
            pageSize);
    }

    public async Task<AgentConversationContextDto?> GetAgentContextAsync(string agentKey, CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(a => a.Key == agentKey, cancellationToken);
        if (agent is null) return null;

        var messages = await dbContext.ChatMessages.Where(m => m.AgentKey == agentKey).OrderByDescending(m => m.CreatedAt).Take(20).OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken);
        var globalMemories = await dbContext.GlobalMemories.Where(m => m.Status == "Approved").Take(6).ToListAsync(cancellationToken);
        
        var agentMemories = agentKey == "main-brain" 
            ? await dbContext.AgentMemories.Where(m => m.Status == "Approved").Take(10).ToListAsync(cancellationToken)
            : await dbContext.AgentMemories.Where(m => m.AgentKey == agentKey && m.Status == "Approved").Take(4).ToListAsync(cancellationToken);
        var videos = await dbContext.Videos.OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken);
        var runs = await dbContext.AgentRuns.Where(r => r.AgentKey == agentKey).OrderByDescending(r => r.QueuedAt).Take(2).ToListAsync(cancellationToken);

        var conn = await dbContext.AgentConnections.FirstOrDefaultAsync(c => c.AgentKey == agentKey, cancellationToken);

        return new AgentConversationContextDto(
            ToAgentSummaryDto(agent, conn, agentMemories.Take(2).Select(m => m.Title).ToArray(), runs.Select(ToRunDto).ToArray()),
            messages.Select(ToChatDto).ToArray(),
            globalMemories.Select(ToMemoryDto).ToArray(),
            agentMemories.Select(ToMemoryDto).ToArray(),
            videos.Where(v => v.Stage == "Backlog").Take(4).Select(ToVideoDto).ToArray(),
            videos.Where(v => v.Stage == "ReadyToUpload").Take(4).Select(ToVideoDto).ToArray());
    }

    public async Task<IReadOnlyList<ChatMessageDto>> SaveAgentExchangeAsync(
        string agentKey, 
        string userMessage, 
        string assistantMessage, 
        int tokensIn, 
        int tokensOut, 
        decimal cost, 
        int durationMs, 
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        dbContext.ChatMessages.Add(new StudioChatMessageEntity { Id = Guid.NewGuid(), AgentKey = agentKey, Role = "user", Content = userMessage, CreatedAt = now });
        dbContext.ChatMessages.Add(new StudioChatMessageEntity { Id = Guid.NewGuid(), AgentKey = agentKey, Role = "assistant", Content = assistantMessage, CreatedAt = now.AddSeconds(1) });
        dbContext.AgentUsages.Add(new StudioAgentUsageEntity { Id = Guid.NewGuid(), AgentKey = agentKey, CapturedAt = now, RequestCount = 1, TokensIn = tokensIn, TokensOut = tokensOut, CostUsd = cost, DurationMs = durationMs });
        var runId = Guid.NewGuid();
        dbContext.AgentRuns.Add(new StudioAgentRunEntity { Id = runId, AgentKey = agentKey, Title = "Interactive chat", Status = "Succeeded", Summary = "Chat guidance", QueuedAt = now, CompletedAt = now.AddSeconds(2) });

        var agent = await dbContext.Agents.FirstOrDefaultAsync(a => a.Key == agentKey, cancellationToken);
        if (agent is not null) { agent.LastRunAt = now; agent.UpdatedAt = now; }

        await dbContext.SaveChangesAsync(cancellationToken);
        await emitter.EmitAgentRunCompletedAsync(new AgentRunCompletedPayload(agentKey, runId, "Succeeded", durationMs), cancellationToken);
        
        cache.Remove("dashboard_summary");
        cache.Remove($"budget_{agentKey}");

        var messages = await dbContext.ChatMessages
            .Where(m => m.AgentKey == agentKey)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(ToChatDto).ToArray();
    }

    public async Task<bool> IsAgentWithinBudgetAsync(string agentKey, CancellationToken cancellationToken)
    {
        var cacheKey = $"budget_{agentKey}";
        if (cache.TryGetValue(cacheKey, out bool withinBudget)) return withinBudget;

        var agent = await dbContext.Agents.FirstOrDefaultAsync(a => a.Key == agentKey, cancellationToken);
        if (agent == null) return true;

        if (agent.DailyTokenBudget <= 0 && agent.MonthlyCostBudget <= 0) return true;

        var now = DateTimeOffset.UtcNow;
        var startOfDay = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        if (agent.DailyTokenBudget > 0)
        {
            var tokensToday = await dbContext.AgentUsages
                .Where(u => u.AgentKey == agentKey && u.CapturedAt >= startOfDay)
                .SumAsync(u => u.TokensIn + u.TokensOut, cancellationToken);
            
            if (tokensToday >= agent.DailyTokenBudget)
            {
                cache.Set(cacheKey, false, TimeSpan.FromMinutes(10));
                return false;
            }
        }

        if (agent.MonthlyCostBudget > 0)
        {
            var costThisMonth = await dbContext.AgentUsages
                .Where(u => u.AgentKey == agentKey && u.CapturedAt >= startOfMonth)
                .SumAsync(u => u.CostUsd, cancellationToken);

            if (costThisMonth >= agent.MonthlyCostBudget)
            {
                cache.Set(cacheKey, false, TimeSpan.FromMinutes(10));
                return false;
            }
        }

        cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
        return true;
    }

    public async Task<MemoryRecordDto?> ReviewMemoryAsync(Guid id, string status, ReviewMemoryRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? expiresAt = request.TtlDays.HasValue ? now.AddDays(request.TtlDays.Value) : null;

        // Try Global first
        var global = await dbContext.GlobalMemories.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (global != null)
        {
            global.Title = request.RevisedTitle ?? global.Title;
            global.Content = request.RevisedContent ?? global.Content;
            global.Status = status;
            global.UpdatedAt = now;
            global.ApprovedAt = status == "Approved" ? now : null;
            global.ExpiresAt = expiresAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToMemoryDto(global);
        }

        // Try Agent second
        var agentMem = await dbContext.AgentMemories.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (agentMem != null)
        {
            agentMem.Title = request.RevisedTitle ?? agentMem.Title;
            agentMem.Content = request.RevisedContent ?? agentMem.Content;
            agentMem.Status = status;
            agentMem.UpdatedAt = now;
            agentMem.ApprovedAt = status == "Approved" ? now : null;
            agentMem.ExpiresAt = expiresAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToMemoryDto(agentMem);
        }

        return null;
    }

    public async Task<IReadOnlyList<MemorySuggestionDto>> GetPendingMemorySuggestionsAsync(CancellationToken cancellationToken)
    {
        var suggestions = await memoryRepository.GetPendingSuggestionsAsync(cancellationToken);
        return suggestions.Select(ToMemorySuggestionDto).ToArray();
    }

    public async Task<VideoItemDto?> UpdateVideoStageAsync(Guid id, UpdateVideoStageRequest request, CancellationToken cancellationToken)
    {
        var video = await dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (video is null) return null;

        video.Stage = request.Stage;
        video.UpdatedAt = DateTimeOffset.UtcNow;
        video.PublishedAt = request.Stage == "Published" ? DateTimeOffset.UtcNow : video.PublishedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        var result = ToVideoDto(video);
        await emitter.EmitStageCompletedAsync(new StageCompletedPayload(result.Id, result.Stage, 1.0), cancellationToken);
        cache.Remove("dashboard_summary");
        return result;
    }

    public async Task<ScheduleJobDto> CreateManualScheduleAsync(CreateManualScheduleRequest request, CancellationToken cancellationToken)
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
            NextRunAt = request.IsEnabled ? DateTimeOffset.UtcNow.AddSeconds(10) : null, 
            CreatedAt = DateTimeOffset.UtcNow 
        };

        dbContext.ScheduleJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (job.IsEnabled)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            
            var quartzJob = JobBuilder.Create<AiContentFactory.Infrastructure.Scheduler.AgentJob>()
                .WithIdentity($"ManualJob-{job.Id}", "Agents")
                .UsingJobData("JobId", job.Id.ToString())
                .Build();

            var quartzTrigger = TriggerBuilder.Create()
                .WithIdentity($"ManualTrigger-{job.Id}", "Agents")
                .StartAt(DateTimeOffset.UtcNow.AddSeconds(5)) // Start almost immediately for manual
                .Build();

            await scheduler.ScheduleJob(quartzJob, quartzTrigger, cancellationToken);
        }

        return ToScheduleJobDto(job);
    }

    public async Task<AgentSettingsDto?> SaveAgentSettingsAsync(string agentKey, SaveAgentSettingsRequest request, CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(a => a.Key == agentKey, cancellationToken);
        if (agent is null) return null;

        var conn = await dbContext.AgentConnections.FirstOrDefaultAsync(c => c.AgentKey == agentKey, cancellationToken);
        if (conn == null)
        {
            conn = new StudioAgentConnectionEntity { Id = Guid.NewGuid(), AgentKey = agentKey };
            dbContext.AgentConnections.Add(conn);
        }

        conn.ProviderName = request.ProviderName; 
        conn.ModelName = request.ModelName; 
        if (!IsMasked(request.ApiKey)) conn.ApiKey = encryption.Encrypt(request.ApiKey);
        conn.ClientId = request.ClientId; 
        if (!IsMasked(request.ClientSecret)) conn.ClientSecret = encryption.Encrypt(request.ClientSecret); 
        conn.RefreshToken = request.RefreshToken;
        conn.UseOpenRouter = request.UseOpenRouter;
        conn.OpenRouterModel = request.OpenRouterModel;
        if (!IsMasked(request.OpenRouterApiKey)) conn.OpenRouterApiKey = encryption.Encrypt(request.OpenRouterApiKey);
        conn.BaseUrl = request.BaseUrl;
        conn.UpdatedAt = DateTimeOffset.UtcNow;

        agent.UpdatedAt = DateTimeOffset.UtcNow;
        agent.IsConnected = !string.IsNullOrEmpty(request.ApiKey) || 
                           !string.IsNullOrEmpty(request.RefreshToken) || 
                           (request.UseOpenRouter && !string.IsNullOrEmpty(request.OpenRouterApiKey));
        agent.Status = agent.IsConnected ? "Connected" : "Connect API first";

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetAgentSettingsAsync(agentKey, cancellationToken);
    }

    public async Task<AgentSettingsDto?> GetAgentSettingsAsync(string agentKey, CancellationToken cancellationToken)
    {
        var agent = await dbContext.Agents.FirstOrDefaultAsync(a => a.Key == agentKey, cancellationToken);
        if (agent == null) return null;

        var conn = await dbContext.AgentConnections.FirstOrDefaultAsync(c => c.AgentKey == agentKey, cancellationToken);
        
        return new AgentSettingsDto(
            agent.Key, 
            agent.Name, 
            agent.Category, 
            agent.RequiresConnection, 
            agent.SupportsOpenRouter, 
            agent.IsConnected, 
            conn?.ProviderName ?? string.Empty, 
            conn?.ModelName ?? string.Empty, 
            conn?.BaseUrl ?? string.Empty, 
            encryption.Decrypt(conn?.ApiKey ?? string.Empty), 
            conn?.ClientId ?? string.Empty, 
            encryption.Decrypt(conn?.ClientSecret ?? string.Empty), 
            conn?.RefreshToken ?? string.Empty, 
            agent.SourceVideoPath, 
            agent.StorageFolderId, 
            agent.StorageFolderName, 
            agent.StorageFolderPath, 
            agent.StorageFolderUrl, 
            conn?.UseOpenRouter ?? false, 
            conn?.OpenRouterModel ?? string.Empty, 
            encryption.Decrypt(conn?.OpenRouterApiKey ?? string.Empty), 
            agent.Notes, 
            agent.UpdatedAt);
    }

    public async Task<VideoItemDto?> LinkVideoToAssetAsync(Guid id, string driveFileId, CancellationToken cancellationToken)
    {
        var video = await dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (video == null) return null;

        video.DriveFileId = driveFileId;
        video.UpdatedAt = DateTimeOffset.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        cache.Remove("dashboard_summary");
        
        var dto = ToVideoDto(video);
        await emitter.EmitStageCompletedAsync(new StageCompletedPayload(id, video.Stage, 1.0), cancellationToken);
        return dto;
    }

    public async Task<DriveSettingsDto> SaveDriveSettingsAsync(SaveDriveSettingsRequest request, CancellationToken cancellationToken)
    {
        var config = await dbContext.DriveConfigs.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            config = new StudioDriveConfigEntity { Id = Guid.NewGuid() };
            dbContext.DriveConfigs.Add(config);
        }

        config.ClientId = request.ClientId;
        config.ClientSecret = encryption.Encrypt(request.ClientSecret);
        config.RefreshToken = request.RefreshToken;
        config.RootFolderId = request.RootFolderId;
        config.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new DriveSettingsDto(config.ClientId, encryption.Decrypt(config.ClientSecret), config.RefreshToken, config.RootFolderId);
    }

    public async Task<DriveSettingsDto> GetDriveSettingsAsync(CancellationToken cancellationToken)
    {
        var config = await dbContext.DriveConfigs.FirstOrDefaultAsync(cancellationToken);
        if (config == null) return new DriveSettingsDto("", "", "", "root");
        return new DriveSettingsDto(config.ClientId, encryption.Decrypt(config.ClientSecret), config.RefreshToken, config.RootFolderId);
    }

    private static UsageSeriesDto[] BuildUsageSeries(IReadOnlyList<StudioAgentEntity> agents, IReadOnlyList<StudioAgentUsageEntity> usages)
    {
        var usageByAgent = usages.GroupBy(u => u.AgentKey).ToDictionary(g => g.Key, g => g.ToList());
        return agents.Select((agent, index) => {
            var agentUsages = usageByAgent.GetValueOrDefault(agent.Key) ?? [];
            return new UsageSeriesDto(
                agent.Key, 
                agent.Name, 
                Palette[index % Palette.Length], 
                agentUsages.TakeLast(7).Select(u => new UsagePointDto(u.CapturedAt, u.RequestCount, u.TokensIn, u.TokensOut, u.CostUsd, u.DurationMs)).ToArray());
        }).ToArray();
    }

    private static PlatformPublicationWidgetDto[] BuildPublicationWidgets(IReadOnlyList<StudioPublicationEntity> publications)
    {
        return publications.GroupBy(p => p.Platform).OrderBy(g => g.Key).Select(g => new PlatformPublicationWidgetDto(g.Key, g.Count(i => i.Status == "Published"), g.Count(i => i.Status == "Scheduled"), g.Count(i => i.Status == "Failed"), g.Where(i => i.Status == "Published").Sum(i => i.Views))).ToArray();
    }

    private static ProviderOptionDto[] BuildProviderOptions() => [new ProviderOptionDto("Brain", ["OpenAI", "Claude", "Gemini", "OpenRouter"]), new ProviderOptionDto("Video", ["Runway", "Pika", "Luma", "Manual"])];

    private static bool IsMasked(string? value) => value != null && value.Contains("...");

    private static AgentSummaryDto ToAgentSummaryDto(StudioAgentEntity a, StudioAgentConnectionEntity? c, IReadOnlyList<string> h, IReadOnlyList<AgentRunDto> r) => new AgentSummaryDto(a.Key, a.Name, a.Description, a.Category, a.RequiresConnection, a.SupportsOpenRouter, a.IsConnected, c?.ProviderName ?? string.Empty, c?.ModelName ?? string.Empty, a.Status, a.CapabilitySummary, a.LastRunAt, h, r);
    private AgentSettingsDto ToSettingsDto(StudioAgentEntity a, StudioAgentConnectionEntity? c) => new AgentSettingsDto(
        a.Key, a.Name, a.Category, a.RequiresConnection, a.SupportsOpenRouter, a.IsConnected, 
        c?.ProviderName ?? string.Empty, c?.ModelName ?? string.Empty, c?.BaseUrl ?? string.Empty, masking.Mask(encryption.Decrypt(c?.ApiKey ?? string.Empty)), 
        c?.ClientId ?? string.Empty, masking.Mask(encryption.Decrypt(c?.ClientSecret ?? string.Empty)), masking.Mask(c?.RefreshToken ?? string.Empty, 2, 2), 
        a.SourceVideoPath, a.StorageFolderId, a.StorageFolderName, a.StorageFolderPath, a.StorageFolderUrl, 
        c?.UseOpenRouter ?? false, c?.OpenRouterModel ?? string.Empty, masking.Mask(encryption.Decrypt(c?.OpenRouterApiKey ?? string.Empty)), 
        a.Notes, a.UpdatedAt);
    private static MemoryRecordDto ToMemoryDto(StudioGlobalMemoryEntity m) => new MemoryRecordDto(m.Id, "Global", null, m.Title, m.Content, m.Status, m.Tags, m.CreatedAt, m.UpdatedAt, m.ApprovedAt, m.ExpiresAt);
    private static MemoryRecordDto ToMemoryDto(StudioAgentMemoryEntity m) => new MemoryRecordDto(m.Id, "Local", m.AgentKey, m.Title, m.Content, m.Status, m.Tags, m.CreatedAt, m.UpdatedAt, m.ApprovedAt, m.ExpiresAt);
    private static VideoItemDto ToVideoDto(StudioVideoEntity v) => new VideoItemDto(v.Id, v.Title, v.Topic, v.Format, v.Stage, v.StorageFolder, v.DriveFileId, v.SourceAgentKey, v.Platforms, v.CreatedAt, v.PublishedAt);
    private static MemorySuggestionDto ToMemorySuggestionDto(MemorySuggestion s) => new MemorySuggestionDto(s.Id, s.Scope.ToString(), s.AgentName, s.Content, s.Reason, s.Status.ToString(), s.CreatedAt);
    private static ScheduleJobDto ToScheduleJobDto(StudioScheduleJobEntity j) => new ScheduleJobDto(j.Id, j.Name, j.Type, j.AgentKey, j.IsEnabled, j.Status, j.Trigger, j.QueueMode, j.NextRunAt, j.LastRunAt, j.Notes);
    private static ChatMessageDto ToChatDto(StudioChatMessageEntity m) => new ChatMessageDto(m.Id, m.AgentKey, m.Role, m.Content, m.CreatedAt);
    private static AgentRunDto ToRunDto(StudioAgentRunEntity r) => new AgentRunDto(r.Id, r.AgentKey, r.Title, r.Status, r.Summary, r.QueuedAt, r.CompletedAt, r.AttemptCount, r.MaxRetries);
}
