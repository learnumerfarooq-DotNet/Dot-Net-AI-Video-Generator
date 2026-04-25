using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Persistence;

public static class StudioDatabaseInitializer
{
    public static async Task InitializeAsync(StudioDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureAgentSchemaAsync(dbContext, cancellationToken);

        if (await dbContext.Agents.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var agents = SeedAgents(now);
        var usages = SeedUsage(agents, now);
        var memories = SeedMemories(now);
        var videos = SeedVideos(now);
        var publications = SeedPublications(videos, now);
        var schedules = SeedSchedules(now);
        var runs = SeedRuns(now);
        var messages = SeedChatMessages(now);

        await dbContext.Agents.AddRangeAsync(agents, cancellationToken);
        await dbContext.AgentUsages.AddRangeAsync(usages, cancellationToken);
        await dbContext.Memories.AddRangeAsync(memories, cancellationToken);
        await dbContext.Videos.AddRangeAsync(videos, cancellationToken);
        await dbContext.Publications.AddRangeAsync(publications, cancellationToken);
        await dbContext.ScheduleJobs.AddRangeAsync(schedules, cancellationToken);
        await dbContext.AgentRuns.AddRangeAsync(runs, cancellationToken);
        await dbContext.ChatMessages.AddRangeAsync(messages, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAgentSchemaAsync(StudioDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE studio_agents
            ADD COLUMN IF NOT EXISTS "StorageFolderName" character varying(200) NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS "StorageFolderPath" character varying(400) NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS "StorageFolderUrl" character varying(500) NOT NULL DEFAULT '';
            """,
            cancellationToken);
    }

    private static List<StudioAgentEntity> SeedAgents(DateTimeOffset now)
    {
        return
        [
            CreateAgent("main-brain", "Main Brain", "Brain", true, true, false, "OpenAI", "gpt-4.1", "Connect GPT API and guide the whole content factory.", "System architect and chat assistant for content decisions.", 1, now),
            CreateAgent("trend-agent", "Trend Agent", "Discovery", true, true, false, "OpenAI", "gpt-4.1-mini", "Detect Angular/.NET trend opportunities and feed the queue.", "Trend discovery, topic ranking, and signal analysis.", 2, now),
            CreateAgent("script-agent", "Script Agent", "Writing", true, true, false, "OpenAI", "gpt-4.1", "Turn strong angles into scripts, hooks, and outlines.", "Long-form scripts, shorts scripts, and hook improvements.", 3, now),
            CreateAgent("video-generation-agent", "Video Generation Agent", "Video", true, false, false, "Runway", "gen-4", "Create production-ready video assets for technical content.", "Scene planning, render settings, and final video generation.", 4, now),
            CreateAgent("shorts-agent-1", "Shorts Agent 1", "Shorts", true, true, false, "OpenAI", "gpt-4.1-mini", "Cut long ideas into short-form concepts.", "Short-form ideation and clip breakdowns.", 5, now),
            CreateAgent("shorts-agent-2", "Shorts Agent 2", "Shorts", true, true, false, "OpenRouter", "openai/gpt-4.1-mini", "Generate alternate shorts angles and remix variations.", "Remixes, hooks, and second-variant shorts.", 6, now),
            CreateAgent("youtube-agent", "YouTube Agent", "Publishing", true, false, false, "YouTube", "youtube-publisher", "Manage YouTube upload and performance workflow.", "Upload planning, descriptions, chapters, and publish flow.", 7, now),
            CreateAgent("tiktok-agent", "TikTok Agent", "Publishing", true, false, false, "TikTok", "tiktok-publisher", "Manage TikTok posting and iteration loop.", "Posting windows, captions, and retries.", 8, now),
            CreateAgent("instagram-agent", "Instagram Agent", "Publishing", true, false, false, "Instagram", "instagram-publisher", "Manage Instagram Reels and post packaging.", "Reels workflow, captions, and publishing status.", 9, now),
            CreateAgent("facebook-agent", "Facebook Agent", "Publishing", true, false, false, "Facebook", "facebook-publisher", "Manage Facebook publishing and repost flow.", "Cross-posting and audience-fit packaging.", 10, now),
            CreateAgent("linkedin-agent", "LinkedIn Agent", "Publishing", true, false, false, "LinkedIn", "linkedin-publisher", "Manage LinkedIn video publishing and thought-leadership angle.", "Professional tone packaging and publish timing.", 11, now)
        ];
    }

    private static StudioAgentEntity CreateAgent(
        string key,
        string name,
        string category,
        bool requiresConnection,
        bool supportsOpenRouter,
        bool isConnected,
        string provider,
        string model,
        string description,
        string capability,
        int sortOrder,
        DateTimeOffset now)
    {
        var hasDriveWorkspace = key.Contains("agent", StringComparison.OrdinalIgnoreCase);
        var driveFolderId = hasDriveWorkspace ? $"gdrive-{key}" : string.Empty;
        var driveFolderPath = hasDriveWorkspace ? $"Google Drive/{name.Replace(' ', '-')}" : string.Empty;

        return new StudioAgentEntity
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = name,
            Description = description,
            Category = category,
            RequiresConnection = requiresConnection,
            SupportsOpenRouter = supportsOpenRouter,
            IsConnected = isConnected,
            ProviderName = provider,
            ModelName = model,
            BaseUrl = string.Empty,
            ApiKey = string.Empty,
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            RefreshToken = string.Empty,
            SourceVideoPath = key is "video-generation-agent" or "shorts-agent-1" or "shorts-agent-2"
                ? $"Google Drive/Input/{name.Replace(' ', '-')}"
                : string.Empty,
            StorageFolderId = driveFolderId,
            StorageFolderName = hasDriveWorkspace ? $"{name} Workspace" : string.Empty,
            StorageFolderPath = driveFolderPath,
            StorageFolderUrl = hasDriveWorkspace ? $"https://drive.google.com/drive/folders/{driveFolderId}" : string.Empty,
            UseOpenRouter = false,
            OpenRouterModel = string.Empty,
            OpenRouterApiKey = string.Empty,
            Status = isConnected ? "Connected" : "Connect API first",
            CapabilitySummary = capability,
            SortOrder = sortOrder,
            LastRunAt = now.AddHours(-sortOrder * 2),
            UpdatedAt = now,
            Notes = "Google Drive folder and API credentials can be set from Settings."
        };
    }

    private static List<StudioAgentUsageEntity> SeedUsage(IReadOnlyList<StudioAgentEntity> agents, DateTimeOffset now)
    {
        var usage = new List<StudioAgentUsageEntity>();
        for (var agentIndex = 0; agentIndex < agents.Count; agentIndex++)
        {
            for (var day = 6; day >= 0; day--)
            {
                usage.Add(new StudioAgentUsageEntity
                {
                    Id = Guid.NewGuid(),
                    AgentKey = agents[agentIndex].Key,
                    CapturedAt = new DateTimeOffset(
                        now.UtcDateTime.Date.AddDays(-day).AddHours(9 + agentIndex % 4),
                        TimeSpan.Zero),
                    RequestCount = 4 + agentIndex + (6 - day),
                    TokensIn = 900 + agentIndex * 110 + day * 75,
                    TokensOut = 1400 + agentIndex * 150 + day * 90,
                    CostUsd = Math.Round(0.48m + agentIndex * 0.09m + (6 - day) * 0.03m, 2),
                    DurationMs = 1300 + agentIndex * 80 + day * 45
                });
            }
        }

        return usage;
    }

    private static List<StudioMemoryEntity> SeedMemories(DateTimeOffset now)
    {
        return
        [
            CreateMemory("Global", null, "Video performance history", "Angular standalone migration content performs better when the first 10 seconds compare old vs new patterns.", "Approved", ["performance", "hooks"], now.AddDays(-22), now.AddDays(-21)),
            CreateMemory("Global", null, "Trending topics", "Signal-based state management and Angular modern architecture topics keep producing good watch time.", "Approved", ["trend", "angular"], now.AddDays(-18), now.AddDays(-18)),
            CreateMemory("Global", null, "Successful hooks", "Hooks with a direct question plus an engineering pain point outperform broad AI-intro hooks.", "Approved", ["hook", "copy"], now.AddDays(-12), now.AddDays(-12)),
            CreateMemory("Global", null, "Audience behavior", "Technical viewers stay longer when examples include folder structure and tradeoffs, not only theory.", "Approved", ["audience", "retention"], now.AddDays(-9), now.AddDays(-9)),
            CreateMemory("Global", null, "Global optimization rules", "Keep technical videos concise, visual, and benchmark-based before adding calls to action.", "Approved", ["optimization"], now.AddDays(-6), now.AddDays(-6)),
            CreateMemory("Local", "script-agent", "Writing style improvement", "Use shorter sentences and label the three biggest architecture decisions explicitly.", "Approved", ["style", "script"], now.AddDays(-8), now.AddDays(-7)),
            CreateMemory("Local", "video-generation-agent", "Rendering settings", "Use brighter editor shots and slower zooms for IDE walkthrough clips.", "Approved", ["video", "rendering"], now.AddDays(-7), now.AddDays(-7)),
            CreateMemory("Local", "youtube-agent", "Best posting time", "YouTube uploads around 7 PM local time have the strongest first-hour velocity.", "Approved", ["youtube", "schedule"], now.AddDays(-5), now.AddDays(-5)),
            CreateMemory("Pending", "trend-agent", "New trend signal", "ASP.NET Core background workers plus AI automation is gaining attention and should be reviewed for global memory.", "Pending", ["trend", "dotnet"], now.AddDays(-1), null),
            CreateMemory("Pending", "script-agent", "Hook variation", "Open with one painful monolith problem before naming the architecture pattern.", "Pending", ["hook", "script"], now.AddHours(-18), null),
            CreateMemory("Pending", "linkedin-agent", "Professional framing", "Lead with team efficiency and maintainability when repackaging for LinkedIn.", "Pending", ["linkedin", "audience"], now.AddHours(-12), null)
        ];
    }

    private static StudioMemoryEntity CreateMemory(
        string scope,
        string? agentKey,
        string title,
        string content,
        string status,
        string[] tags,
        DateTimeOffset createdAt,
        DateTimeOffset? approvedAt)
    {
        return new StudioMemoryEntity
        {
            Id = Guid.NewGuid(),
            Scope = scope == "Pending" ? "Global" : scope,
            AgentKey = agentKey,
            Title = title,
            Content = content,
            Status = status,
            Tags = tags,
            CreatedAt = createdAt,
            UpdatedAt = approvedAt ?? createdAt,
            ApprovedAt = approvedAt
        };
    }

    private static List<StudioVideoEntity> SeedVideos(DateTimeOffset now)
    {
        return
        [
            CreateVideo("Angular Signals vs NgRx for Dashboard State", "Angular signals for AI dashboards", "Longform", "ReadyToUpload", "Google Drive / Ready to Upload", "drive-ready-001", "video-generation-agent", ["youtube", "linkedin"], now.AddDays(-2), null),
            CreateVideo(".NET Worker + Queue Architecture for AI Agents", "Background workers and queued execution", "Longform", "ReadyToUpload", "Google Drive / Ready to Upload", "drive-ready-002", "video-generation-agent", ["youtube", "facebook"], now.AddDays(-1), null),
            CreateVideo("3 Short Hooks for Angular Architects", "Hooks for frontend architects", "Short", "ReadyToUpload", "Google Drive / Ready to Upload", "drive-ready-003", "shorts-agent-1", ["youtube", "instagram", "tiktok"], now.AddHours(-18), null),
            CreateVideo("Postgres Memory System for Multi-Agent Apps", "Postgres memory design", "Longform", "Backlog", "Google Drive / Backlog", "drive-backlog-001", "script-agent", ["youtube"], now.AddDays(-4), null),
            CreateVideo("OpenRouter Fallback Strategy for Content Agents", "OpenRouter as backup provider", "Short", "Backlog", "Google Drive / Backlog", "drive-backlog-002", "shorts-agent-2", ["linkedin", "tiktok"], now.AddDays(-3), null),
            CreateVideo("Scheduler Retry Pattern for Social Uploads", "Retry failed uploads pattern", "Short", "Backlog", "Google Drive / Backlog", "drive-backlog-003", "trend-agent", ["youtube", "linkedin"], now.AddDays(-2), null),
            CreateVideo("Angular Clean Architecture Dashboard", "Dashboard structure and widgets", "Longform", "Published", "Google Drive / Published", "drive-published-001", "video-generation-agent", ["youtube", "linkedin"], now.AddDays(-16), now.AddDays(-14)),
            CreateVideo(".NET AI Agent Scheduler", "Scheduler system walkthrough", "Short", "Published", "Google Drive / Published", "drive-published-002", "video-generation-agent", ["youtube", "tiktok"], now.AddDays(-12), now.AddDays(-11)),
            CreateVideo("Why Global Memory Needs Approval", "Memory approval workflow", "Short", "Published", "Google Drive / Published", "drive-published-003", "shorts-agent-1", ["instagram", "facebook"], now.AddDays(-9), now.AddDays(-8))
        ];
    }

    private static StudioVideoEntity CreateVideo(
        string title,
        string topic,
        string format,
        string stage,
        string storageFolder,
        string driveFileId,
        string sourceAgentKey,
        string[] platforms,
        DateTimeOffset createdAt,
        DateTimeOffset? publishedAt)
    {
        return new StudioVideoEntity
        {
            Id = Guid.NewGuid(),
            Title = title,
            Topic = topic,
            Format = format,
            Stage = stage,
            StorageFolder = storageFolder,
            DriveFileId = driveFileId,
            SourceAgentKey = sourceAgentKey,
            Platforms = platforms,
            CreatedAt = createdAt,
            UpdatedAt = publishedAt ?? createdAt,
            PublishedAt = publishedAt
        };
    }

    private static List<StudioPublicationEntity> SeedPublications(IReadOnlyList<StudioVideoEntity> videos, DateTimeOffset now)
    {
        var publishedVideos = videos.Where(video => video.Stage == "Published").ToArray();

        return
        [
            CreatePublication(publishedVideos[0], "YouTube", "Published", 18300, 1240, 148, 66, now.AddDays(-14)),
            CreatePublication(publishedVideos[0], "LinkedIn", "Published", 6200, 540, 42, 31, now.AddDays(-14).AddHours(2)),
            CreatePublication(publishedVideos[1], "YouTube", "Published", 12100, 880, 95, 44, now.AddDays(-11)),
            CreatePublication(publishedVideos[1], "TikTok", "Published", 19800, 1500, 172, 86, now.AddDays(-10)),
            CreatePublication(publishedVideos[2], "Instagram", "Published", 8700, 790, 60, 39, now.AddDays(-8)),
            CreatePublication(publishedVideos[2], "Facebook", "Failed", 0, 0, 0, 0, null),
            new StudioPublicationEntity
            {
                Id = Guid.NewGuid(),
                VideoId = videos.First(video => video.Stage == "ReadyToUpload").Id,
                Platform = "YouTube",
                Status = "Scheduled",
                PublishedUrl = string.Empty,
                PublishedAt = null
            },
            new StudioPublicationEntity
            {
                Id = Guid.NewGuid(),
                VideoId = videos.First(video => video.Stage == "Backlog").Id,
                Platform = "LinkedIn",
                Status = "Scheduled",
                PublishedUrl = string.Empty,
                PublishedAt = null
            }
        ];
    }

    private static StudioPublicationEntity CreatePublication(
        StudioVideoEntity video,
        string platform,
        string status,
        long views,
        long likes,
        long comments,
        long shares,
        DateTimeOffset? publishedAt)
    {
        return new StudioPublicationEntity
        {
            Id = Guid.NewGuid(),
            VideoId = video.Id,
            Platform = platform,
            Status = status,
            PublishedUrl = publishedAt is null ? string.Empty : $"https://example.com/{platform.ToLowerInvariant()}/{video.Id}",
            Views = views,
            Likes = likes,
            Comments = comments,
            Shares = shares,
            PublishedAt = publishedAt
        };
    }

    private static List<StudioScheduleJobEntity> SeedSchedules(DateTimeOffset now)
    {
        return
        [
            CreateSchedule("Weekly backlog review", "Manual", "main-brain", true, "Queued", "Every Friday 6 PM", "Manual", now.AddDays(1), now.AddDays(-6), "Manual scheduling slot for content review.", now),
            CreateSchedule("Trend scan and queue fill", "DailyPosting", "trend-agent", true, "Active", "0 8 * * *", "Recurring", now.AddHours(14), now.AddHours(-10), "Trend agent assigns work for the day.", now),
            CreateSchedule("Retry failed uploads", "RetryUploads", "youtube-agent", true, "Active", "0 */3 * * *", "RetryQueue", now.AddHours(3), now.AddHours(-1), "Retry failed social uploads every three hours.", now),
            CreateSchedule("Queue based execution", "QueueExecution", "video-generation-agent", true, "Active", "Continuous", "WorkQueue", now.AddMinutes(15), now.AddMinutes(-30), "Workers consume queued render and publish jobs.", now),
            CreateSchedule("LinkedIn repost window", "DailyPosting", "linkedin-agent", true, "Active", "0 17 * * 1-5", "Recurring", now.AddHours(7), now.AddHours(-20), "Weekday professional-posting window.", now)
        ];
    }

    private static StudioScheduleJobEntity CreateSchedule(
        string name,
        string type,
        string agentKey,
        bool isEnabled,
        string status,
        string trigger,
        string queueMode,
        DateTimeOffset? nextRunAt,
        DateTimeOffset? lastRunAt,
        string notes,
        DateTimeOffset createdAt)
    {
        return new StudioScheduleJobEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            AgentKey = agentKey,
            IsEnabled = isEnabled,
            Status = status,
            Trigger = trigger,
            QueueMode = queueMode,
            NextRunAt = nextRunAt,
            LastRunAt = lastRunAt,
            Notes = notes,
            CreatedAt = createdAt
        };
    }

    private static List<StudioAgentRunEntity> SeedRuns(DateTimeOffset now)
    {
        return
        [
            CreateRun("trend-agent", "Trend discovery sweep", "Succeeded", "Ranked 12 Angular/.NET topics and passed 4 to Script Agent.", now.AddHours(-26), now.AddHours(-25)),
            CreateRun("script-agent", "Script generation batch", "Succeeded", "Prepared one longform script and two short hook variants.", now.AddHours(-20), now.AddHours(-19)),
            CreateRun("video-generation-agent", "Render batch", "Running", "Rendering 2 videos and packaging 1 short for upload.", now.AddHours(-3), null),
            CreateRun("youtube-agent", "Upload preparation", "Queued", "Waiting for provider connection and ready-video approval.", now.AddHours(-1), null)
        ];
    }

    private static StudioAgentRunEntity CreateRun(
        string agentKey,
        string title,
        string status,
        string summary,
        DateTimeOffset queuedAt,
        DateTimeOffset? completedAt)
    {
        return new StudioAgentRunEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            Title = title,
            Status = status,
            Summary = summary,
            QueuedAt = queuedAt,
            CompletedAt = completedAt
        };
    }

    private static List<StudioChatMessageEntity> SeedChatMessages(DateTimeOffset now)
    {
        return
        [
            new StudioChatMessageEntity
            {
                Id = Guid.NewGuid(),
                AgentKey = "main-brain",
                Role = "assistant",
                Content = "Connect the Main Brain provider, then ask for a longform .NET/Angular content plan and I will help coordinate the rest of the agents.",
                CreatedAt = now.AddHours(-30)
            },
            new StudioChatMessageEntity
            {
                Id = Guid.NewGuid(),
                AgentKey = "main-brain",
                Role = "assistant",
                Content = "Use the scheduler to let Trend Agent fill the queue daily, then approve only the memories that are reusable across multiple videos.",
                CreatedAt = now.AddHours(-6)
            }
        ];
    }
}
