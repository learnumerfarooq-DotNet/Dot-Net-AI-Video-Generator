using Microsoft.EntityFrameworkCore;
using AiContentFactory.Infrastructure.Security;
using AiContentFactory.Infrastructure.Decisions;
using AiContentFactory.Domain.Errors;
using AiContentFactory.Domain.Brain;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Memory.AgentMemories;

namespace AiContentFactory.Infrastructure.Persistence;

public static class StudioDatabaseInitializer
{
    public static async Task InitializeAsync(StudioDbContext dbContext, IEncryptionService encryption, CancellationToken cancellationToken)
    {
        Console.WriteLine("[DB] Initializing database...");
        
        // Diagnostic: list existing tables
        try {
            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
            var tables = new List<string>();
            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken)) {
                while (await reader.ReadAsync(cancellationToken)) tables.Add(reader.GetString(0));
            }
            Console.WriteLine($"[DB] Existing tables: {string.Join(", ", tables)}");
        } catch (Exception ex) {
            Console.WriteLine($"[DB] Could not list tables: {ex.Message}");
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
        Console.WriteLine("[DB] Migrations applied.");
        
        await EnsureAgentSchemaAsync(dbContext, cancellationToken);
        await MigrateLegacyMemoriesAsync(dbContext, cancellationToken);
        await MigrateDriveConfigAsync(dbContext, cancellationToken);
        await MigrateAgentConnectionsAsync(dbContext, encryption, cancellationToken);
        await CleanupAgentSchemaAsync(dbContext, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        List<StudioAgentEntity> agents;
        if (!await dbContext.Agents.AnyAsync(cancellationToken))
        {
            agents = SeedAgents(now);
            var connections = SeedConnections(now);
            var usages = SeedUsage(agents, now);
            var globalMemories = SeedGlobalMemories(now);
            var agentMemories = SeedAgentMemories(now);
            var videos = SeedVideos(now);
            var publications = SeedPublications(videos, now);
            var schedules = SeedSchedules(now);
            var runs = SeedRuns(now);
            var messages = SeedChatMessages(now);

            await dbContext.Agents.AddRangeAsync(agents, cancellationToken);
            await dbContext.AgentConnections.AddRangeAsync(connections, cancellationToken);
            await dbContext.AgentUsages.AddRangeAsync(usages, cancellationToken);
            await dbContext.GlobalMemories.AddRangeAsync(globalMemories, cancellationToken);
            await dbContext.AgentMemories.AddRangeAsync(agentMemories, cancellationToken);
            await dbContext.Videos.AddRangeAsync(videos, cancellationToken);
            await dbContext.Publications.AddRangeAsync(publications, cancellationToken);
            await dbContext.ScheduleJobs.AddRangeAsync(schedules, cancellationToken);
            await dbContext.AgentRuns.AddRangeAsync(runs, cancellationToken);
            await dbContext.ChatMessages.AddRangeAsync(messages, cancellationToken);
        }
        else
        {
            agents = await dbContext.Agents.AsNoTracking().ToListAsync(cancellationToken);
        }

        // V2 Seeding
        if (!await dbContext.RetryPolicies.AnyAsync(cancellationToken))
        {
            var policies = agents.Select(a => new RetryPolicy 
            { 
                Id = Guid.NewGuid(), 
                AgentKey = a.Key, 
                MaxRetries = 3, 
                BackoffSeconds = new() { 30, 120, 300 }, 
                LastUpdated = now 
            });
            await dbContext.RetryPolicies.AddRangeAsync(policies, cancellationToken);
        }

        if (!await dbContext.CircuitBreakerStates.AnyAsync(cancellationToken))
        {
            var cbStates = agents.Select(a => new CircuitBreakerState 
            { 
                Id = Guid.NewGuid(), 
                AgentKey = a.Key, 
                State = "Closed", 
                Threshold = 3, 
                PauseMinutes = 10 
            });
            await dbContext.CircuitBreakerStates.AddRangeAsync(cbStates, cancellationToken);
        }

        if (!await dbContext.BrainStates.AnyAsync(cancellationToken))
        {
            var brainState = new BrainState 
            { 
                Id = Guid.NewGuid(), 
                Status = BrainStatus.Idle, 
                CurrentTickNumber = 0, 
                GlobalMemoryVersion = "1.0",
                LastTickAt = now,
                AgentHealthMap = new Dictionary<string, AgentHealthStatus>()
            };
            await dbContext.BrainStates.AddAsync(brainState, cancellationToken);
        }

        if (!await dbContext.AgentLocalMemories.AnyAsync(cancellationToken))
        {
            var localMemories = agents.Select(a => new AgentLocalMemory 
            { 
                Id = Guid.NewGuid(), 
                AgentKey = a.Key, 
                AgentDisplayName = a.Name,
                ConfigJson = "{}",
                CreatedAt = now,
                UpdatedAt = now
            });
            await dbContext.AgentLocalMemories.AddRangeAsync(localMemories, cancellationToken);
        }

        // Seed Prompt Templates
        if (!await dbContext.PromptTemplates.AnyAsync(cancellationToken))
        {
            await dbContext.PromptTemplates.AddRangeAsync(DefaultPrompts.GetDefaults(), cancellationToken);
        }

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

    private static async Task MigrateLegacyMemoriesAsync(StudioDbContext dbContext, CancellationToken cancellationToken)
    {
        try 
        {
            // Check if legacy table exists
            var checkSql = "SELECT count(*) FROM information_schema.tables WHERE table_name = 'studio_memories'";
            using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = checkSql;
            if (command.Connection!.State != System.Data.ConnectionState.Open) await command.Connection.OpenAsync(cancellationToken);
            var exists = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0) > 0;

            if (!exists) return;

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO studio_global_memories ("Id", "Title", "Content", "Status", "Tags", "CreatedAt", "UpdatedAt", "ApprovedAt")
                SELECT "Id", "Title", "Content", "Status", "Tags", "CreatedAt", "UpdatedAt", "ApprovedAt"
                FROM studio_memories WHERE "Scope" = 'Global';

                INSERT INTO studio_agent_memories ("Id", "AgentKey", "Title", "Content", "Status", "Tags", "CreatedAt", "UpdatedAt", "ApprovedAt")
                SELECT "Id", "AgentKey", "Title", "Content", "Status", "Tags", "CreatedAt", "UpdatedAt", "ApprovedAt"
                FROM studio_memories WHERE "Scope" = 'Local';

                DROP TABLE studio_memories;
                """, cancellationToken);
        }
        catch
        {
            // Ignore if migration fails (e.g. columns don't match)
        }
    }

    private static async Task MigrateDriveConfigAsync(StudioDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.DriveConfigs.AnyAsync(cancellationToken)) return;

            // Attempt to find the config file in common locations
            var paths = new[] 
            { 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "drive.config.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drive.config.json"),
                "data/drive.config.json",
                "drive.config.json"
            };

            string? foundPath = null;
            foreach (var p in paths)
            {
                if (File.Exists(p)) { foundPath = p; break; }
            }

            if (foundPath == null) return;

            var json = await File.ReadAllTextAsync(foundPath, cancellationToken);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var config = new StudioDriveConfigEntity
            {
                Id = Guid.NewGuid(),
                ClientId = GetProp(root, "clientId"),
                ClientSecret = GetProp(root, "clientSecret"),
                RefreshToken = GetProp(root, "refreshToken"),
                RootFolderId = GetProp(root, "rootFolderId") ?? "root",
                UpdatedAt = DateTimeOffset.UtcNow
            };

            dbContext.DriveConfigs.Add(config);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch { /* ignore */ }
    }

    private static string GetProp(System.Text.Json.JsonElement el, string name) 
        => el.TryGetProperty(name, out var p) ? p.GetString() ?? "" : "";

    private static async Task MigrateAgentConnectionsAsync(StudioDbContext dbContext, IEncryptionService encryption, CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.AgentConnections.AnyAsync(cancellationToken)) return;
            if (!await dbContext.Agents.AnyAsync(cancellationToken)) return;

            using var conn = dbContext.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT \"Key\", \"ProviderName\", \"ModelName\", \"BaseUrl\", \"ApiKey\", \"ClientId\", \"ClientSecret\", \"RefreshToken\", \"UseOpenRouter\", \"OpenRouterModel\", \"OpenRouterApiKey\" FROM studio_agents";
            
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var agentKey = reader.GetString(0);
                dbContext.AgentConnections.Add(new StudioAgentConnectionEntity
                {
                    Id = Guid.NewGuid(),
                    AgentKey = agentKey,
                    ProviderName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    ModelName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    BaseUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    ApiKey = encryption.Encrypt(reader.IsDBNull(4) ? string.Empty : reader.GetString(4)),
                    ClientId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    ClientSecret = encryption.Encrypt(reader.IsDBNull(6) ? string.Empty : reader.GetString(6)),
                    RefreshToken = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    UseOpenRouter = !reader.IsDBNull(8) && reader.GetBoolean(8),
                    OpenRouterModel = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    OpenRouterApiKey = encryption.Encrypt(reader.IsDBNull(10) ? string.Empty : reader.GetString(10)),
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch { /* ignore if columns don't exist yet */ }
    }

    private static async Task CleanupAgentSchemaAsync(StudioDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            // Only cleanup if connections have been migrated and table exists
            if (!await dbContext.AgentConnections.AnyAsync(cancellationToken)) return;

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE studio_agents
                DROP COLUMN IF EXISTS "ProviderName",
                DROP COLUMN IF EXISTS "ModelName",
                DROP COLUMN IF EXISTS "BaseUrl",
                DROP COLUMN IF EXISTS "ApiKey",
                DROP COLUMN IF EXISTS "ClientId",
                DROP COLUMN IF EXISTS "ClientSecret",
                DROP COLUMN IF EXISTS "RefreshToken",
                DROP COLUMN IF EXISTS "UseOpenRouter",
                DROP COLUMN IF EXISTS "OpenRouterModel",
                DROP COLUMN IF EXISTS "OpenRouterApiKey";
                """, cancellationToken);
        }
        catch { /* ignore */ }
    }

    private static List<StudioAgentEntity> SeedAgents(DateTimeOffset now)
    {
        return
        [
            CreateAgent("main-brain", "Main Brain", "Brain", true, true, false, "Connect GPT API and guide the whole content factory.", "System architect and chat assistant for content decisions.", 1, now),
            CreateAgent("trend-agent", "Trend Agent", "Discovery", true, true, false, "Detect Angular/.NET trend opportunities and feed the queue.", "Trend discovery, topic ranking, and signal analysis.", 2, now),
            CreateAgent("script-agent", "Script Agent", "Writing", true, true, false, "Turn strong angles into scripts, hooks, and outlines.", "Long-form scripts, shorts scripts, and hook improvements.", 3, now),
            CreateAgent("video-generation-agent", "Video Generation Agent", "Video", true, false, false, "Create production-ready video assets for technical content.", "Scene planning, render settings, and final video generation.", 4, now),
            CreateAgent("shorts-agent-1", "Shorts Agent 1", "Shorts", true, true, false, "Cut long ideas into short-form concepts.", "Short-form ideation and clip breakdowns.", 5, now),
            CreateAgent("shorts-agent-2", "Shorts Agent 2", "Shorts", true, true, true, "Generate alternate shorts angles and remix variations.", "Remixes, hooks, and second-variant shorts.", 6, now),
            CreateAgent("youtube-agent", "YouTube Agent", "Publishing", true, false, false, "Manage YouTube upload and performance workflow.", "Upload planning, descriptions, chapters, and publish flow.", 7, now),
            CreateAgent("tiktok-agent", "TikTok Agent", "Publishing", true, false, false, "Manage TikTok posting and iteration loop.", "Posting windows, captions, and retries.", 8, now),
            CreateAgent("instagram-agent", "Instagram Agent", "Publishing", true, false, false, "Manage Instagram Reels and post packaging.", "Reels workflow, captions, and publishing status.", 9, now),
            CreateAgent("facebook-agent", "Facebook Agent", "Publishing", true, false, false, "Manage Facebook publishing and repost flow.", "Cross-posting and audience-fit packaging.", 10, now),
            CreateAgent("linkedin-agent", "LinkedIn Agent", "Publishing", true, false, false, "Manage LinkedIn video publishing and thought-leadership angle.", "Professional tone packaging and publish timing.", 11, now)
        ];
    }

    private static StudioAgentEntity CreateAgent(
        string key,
        string name,
        string category,
        bool requiresConnection,
        bool supportsOpenRouter,
        bool isConnected,
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
            SourceVideoPath = key is "video-generation-agent" or "shorts-agent-1" or "shorts-agent-2"
                ? $"Google Drive/Input/{name.Replace(' ', '-')}"
                : string.Empty,
            StorageFolderId = driveFolderId,
            StorageFolderName = hasDriveWorkspace ? $"{name} Workspace" : string.Empty,
            StorageFolderPath = driveFolderPath,
            StorageFolderUrl = hasDriveWorkspace ? $"https://drive.google.com/drive/folders/{driveFolderId}" : string.Empty,
            Status = isConnected ? "Connected" : "Connect API first",
            CapabilitySummary = capability,
            SortOrder = sortOrder,
            LastRunAt = now.AddHours(-sortOrder * 2),
            UpdatedAt = now,
            Notes = "Google Drive folder and API credentials can be set from Settings."
        };
    }

    private static List<StudioAgentConnectionEntity> SeedConnections(DateTimeOffset now)
    {
        return
        [
            CreateConnection("main-brain", "OpenAI", "gpt-4.1", now),
            CreateConnection("trend-agent", "OpenAI", "gpt-4.1-mini", now),
            CreateConnection("script-agent", "OpenAI", "gpt-4.1", now),
            CreateConnection("video-generation-agent", "Runway", "gen-4", now),
            CreateConnection("shorts-agent-1", "OpenAI", "gpt-4.1-mini", now),
            CreateConnection("shorts-agent-2", "OpenRouter", "openai/gpt-4.1-mini", now),
            CreateConnection("youtube-agent", "YouTube", "youtube-publisher", now),
            CreateConnection("tiktok-agent", "TikTok", "tiktok-publisher", now),
            CreateConnection("instagram-agent", "Instagram", "instagram-publisher", now),
            CreateConnection("facebook-agent", "Facebook", "facebook-publisher", now),
            CreateConnection("linkedin-agent", "LinkedIn", "linkedin-publisher", now)
        ];
    }

    private static StudioAgentConnectionEntity CreateConnection(string agentKey, string provider, string model, DateTimeOffset now)
    {
        return new StudioAgentConnectionEntity
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            ProviderName = provider,
            ModelName = model,
            UpdatedAt = now
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

    private static List<StudioGlobalMemoryEntity> SeedGlobalMemories(DateTimeOffset now)
    {
        return
        [
            new StudioGlobalMemoryEntity { Id = Guid.NewGuid(), Title = "Video performance history", Content = "Angular standalone migration content performs better when the first 10 seconds compare old vs new patterns.", Status = "Approved", Tags = ["performance", "hooks"], CreatedAt = now.AddDays(-22), UpdatedAt = now.AddDays(-21), ApprovedAt = now.AddDays(-21) },
            new StudioGlobalMemoryEntity { Id = Guid.NewGuid(), Title = "Trending topics", Content = "Signal-based state management and Angular modern architecture topics keep producing good watch time.", Status = "Approved", Tags = ["trend", "angular"], CreatedAt = now.AddDays(-18), UpdatedAt = now.AddDays(-18), ApprovedAt = now.AddDays(-18) },
            new StudioGlobalMemoryEntity { Id = Guid.NewGuid(), Title = "Successful hooks", Content = "Hooks with a direct question plus an engineering pain point outperform broad AI-intro hooks.", Status = "Approved", Tags = ["hook", "copy"], CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12), ApprovedAt = now.AddDays(-12) },
            new StudioGlobalMemoryEntity { Id = Guid.NewGuid(), Title = "Audience behavior", Content = "Technical viewers stay longer when examples include folder structure and tradeoffs, not only theory.", Status = "Approved", Tags = ["audience", "retention"], CreatedAt = now.AddDays(-9), UpdatedAt = now.AddDays(-9), ApprovedAt = now.AddDays(-9) },
            new StudioGlobalMemoryEntity { Id = Guid.NewGuid(), Title = "Global optimization rules", Content = "Keep technical videos concise, visual, and benchmark-based before adding calls to action.", Status = "Approved", Tags = ["optimization"], CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-6), ApprovedAt = now.AddDays(-6) }
        ];
    }

    private static List<StudioAgentMemoryEntity> SeedAgentMemories(DateTimeOffset now)
    {
        return
        [
            new StudioAgentMemoryEntity { Id = Guid.NewGuid(), AgentKey = "script-agent", Title = "Writing style improvement", Content = "Use shorter sentences and label the three biggest architecture decisions explicitly.", Status = "Approved", Tags = ["style", "script"], CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-7), ApprovedAt = now.AddDays(-7) },
            new StudioAgentMemoryEntity { Id = Guid.NewGuid(), AgentKey = "video-generation-agent", Title = "Rendering settings", Content = "Use brighter editor shots and slower zooms for IDE walkthrough clips.", Status = "Approved", Tags = ["video", "rendering"], CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7), ApprovedAt = now.AddDays(-7) },
            new StudioAgentMemoryEntity { Id = Guid.NewGuid(), AgentKey = "youtube-agent", Title = "Best posting time", Content = "YouTube uploads around 7 PM local time have the strongest first-hour velocity.", Status = "Approved", Tags = ["youtube", "schedule"], CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5), ApprovedAt = now.AddDays(-5) },
            new StudioAgentMemoryEntity { Id = Guid.NewGuid(), AgentKey = "trend-agent", Title = "New trend signal", Content = "ASP.NET Core background workers plus AI automation is gaining attention.", Status = "Pending", Tags = ["trend", "dotnet"], CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1) }
        ];
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
            CreatePublication(publishedVideos[2], "Facebook", "Failed", 0, 0, 0, 0, null)
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
