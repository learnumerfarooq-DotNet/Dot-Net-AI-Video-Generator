using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Domain.Brain;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Domain.Memory.AgentMemories;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;
using AiContentFactory.Domain.Trends;
using AiContentFactory.Domain.Analytics;
using AiContentFactory.Domain.Errors;
using AiContentFactory.Domain.Publishing.YouTube;
using AiContentFactory.Domain.Publishing.TikTok;
using AiContentFactory.Domain.Publishing.Instagram;
using AiContentFactory.Domain.Publishing.Facebook;
using AiContentFactory.Domain.Publishing.LinkedIn;
using Microsoft.EntityFrameworkCore;
namespace AiContentFactory.Infrastructure.Persistence;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options) : DbContext(options)
{
    public DbSet<StudioAgentEntity> Agents => Set<StudioAgentEntity>();
    public DbSet<StudioAgentUsageEntity> AgentUsages => Set<StudioAgentUsageEntity>();
    public DbSet<StudioGlobalMemoryEntity> GlobalMemories => Set<StudioGlobalMemoryEntity>();
    public DbSet<StudioAgentMemoryEntity> AgentMemories => Set<StudioAgentMemoryEntity>();
    public DbSet<StudioVideoEntity> Videos => Set<StudioVideoEntity>();
    public DbSet<StudioPublicationEntity> Publications => Set<StudioPublicationEntity>();
    public DbSet<StudioScheduleJobEntity> ScheduleJobs => Set<StudioScheduleJobEntity>();
    public DbSet<StudioChatMessageEntity> ChatMessages => Set<StudioChatMessageEntity>();
    public DbSet<StudioAgentConnectionEntity> AgentConnections => Set<StudioAgentConnectionEntity>();
    public DbSet<StudioAgentRunEntity> AgentRuns => Set<StudioAgentRunEntity>();
    public DbSet<StudioDriveConfigEntity> DriveConfigs => Set<StudioDriveConfigEntity>();

    // Video Pipeline Entities
    public DbSet<VideoPipelineJob> VideoPipelineJobs { get; set; }
    public DbSet<PipelineStage> PipelineStages { get; set; }
    public DbSet<VideoMetadata> VideoMetadata { get; set; }
    public DbSet<PlatformPublishJob> PlatformPublishJobs { get; set; }
    public DbSet<UploadSchedule> UploadSchedules { get; set; }
    public DbSet<AiContentFactory.Domain.Analytics.VideoAnalytics> VideoAnalytics { get; set; }
    public DbSet<AiContentFactory.Domain.Analytics.ViralPattern> ViralPatterns { get; set; }
    public DbSet<PipelineError> ErrorQueue { get; set; }

    // Decision Layer Entities
    public DbSet<AgentDecision> AgentDecisions { get; set; }
    public DbSet<PromptTemplate> PromptTemplates { get; set; }
    public DbSet<DecisionValidation> DecisionValidations { get; set; }
    public DbSet<DecisionCacheEntry> DecisionCacheEntries { get; set; }

    // Phase 1: Foundation Entities
    public DbSet<BrainState> BrainStates { get; set; }
    public DbSet<BrainTickLog> BrainTickLogs { get; set; }
    public DbSet<AgentLocalMemory> AgentLocalMemories { get; set; }
    
    // Phase 2: Pipeline Agents
    public DbSet<ScriptOutput> ScriptOutputs { get; set; }
    public DbSet<EditPlan> EditPlans { get; set; }
    public DbSet<VideoAnalysisResult> VideoAnalysisResults { get; set; }
    public DbSet<ShortClip> ShortClips { get; set; }
    public DbSet<ShortEditPlan> ShortEditPlans { get; set; }
    
    // Phase 3 & 4: Intelligence & Publishing
    public DbSet<TrendResult> TrendResults { get; set; }
    public DbSet<ScrapeResult> ScrapeResults { get; set; }
    public DbSet<UploadPackage> UploadPackages { get; set; }
    public DbSet<AnalyticsReport> AnalyticsReports { get; set; }
    
    // Phase 5: Error Resilience
    public DbSet<DeadLetterEntry> DeadLetterEntries { get; set; }
    public DbSet<CircuitBreakerState> CircuitBreakerStates { get; set; }
    public DbSet<RetryPolicy> RetryPolicies { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    
    // Phase 6: Additional
    public DbSet<DecisionAuditLog> DecisionAuditLogs { get; set; }
    public DbSet<YouTubeUploadResult> YouTubeUploadResults { get; set; }
    public DbSet<YouTubeCredential> YouTubeCredentials { get; set; }
    public DbSet<YouTubeVideoDetails> YouTubeVideoDetails { get; set; }
    public DbSet<TikTokUploadResult> TikTokUploadResults { get; set; }
    public DbSet<TikTokCredential> TikTokCredentials { get; set; }
    public DbSet<InstagramUploadResult> InstagramUploadResults { get; set; }
    public DbSet<InstagramCredential> InstagramCredentials { get; set; }
    public DbSet<FacebookUploadResult> FacebookUploadResults { get; set; }
    public DbSet<FacebookCredential> FacebookCredentials { get; set; }
    public DbSet<LinkedInUploadResult> LinkedInUploadResults { get; set; }
    public DbSet<LinkedInCredential> LinkedInCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<StudioAgentEntity>(entity =>
        {
            entity.ToTable("studio_agents");
            entity.HasKey(agent => agent.Id);
            entity.HasIndex(agent => agent.Key).IsUnique();
            entity.Property(agent => agent.Key).HasMaxLength(120);
            entity.Property(agent => agent.Name).HasMaxLength(200);
            entity.Property(agent => agent.Category).HasMaxLength(100);
            entity.Property(agent => agent.SourceVideoPath).HasMaxLength(300);
            entity.Property(agent => agent.StorageFolderId).HasMaxLength(200);
            entity.Property(agent => agent.StorageFolderName).HasMaxLength(200);
            entity.Property(agent => agent.StorageFolderPath).HasMaxLength(400);
            entity.Property(agent => agent.StorageFolderUrl).HasMaxLength(500);
            entity.Property(agent => agent.Status).HasMaxLength(120);
        });

        modelBuilder.Entity<StudioAgentUsageEntity>(entity =>
        {
            entity.ToTable("studio_agent_usages");
            entity.HasKey(usage => usage.Id);
            entity.HasIndex(usage => new { usage.AgentKey, usage.CapturedAt });
            entity.Property(usage => usage.AgentKey).HasMaxLength(120);
            entity.Property(usage => usage.CostUsd).HasColumnType("numeric(12,4)");
        });

        modelBuilder.Entity<StudioGlobalMemoryEntity>(entity =>
        {
            entity.ToTable("studio_global_memories");
            entity.HasKey(memory => memory.Id);
            entity.HasIndex(memory => memory.Status);
            entity.Property(memory => memory.Title).HasMaxLength(240);
            entity.Property(memory => memory.Status).HasMaxLength(40);
            entity.Property(memory => memory.Tags).HasColumnType("text[]");
            entity.Property(memory => memory.Embedding).HasColumnType("real[]");
        });

        modelBuilder.Entity<StudioAgentMemoryEntity>(entity =>
        {
            entity.ToTable("studio_agent_memories");
            entity.HasKey(memory => memory.Id);
            entity.HasIndex(memory => new { memory.AgentKey, memory.Status });
            entity.Property(memory => memory.AgentKey).HasMaxLength(120);
            entity.Property(memory => memory.Title).HasMaxLength(240);
            entity.Property(memory => memory.Status).HasMaxLength(40);
            entity.Property(memory => memory.Tags).HasColumnType("text[]");
            entity.Property(memory => memory.Embedding).HasColumnType("real[]");
        });

        modelBuilder.Entity<StudioVideoEntity>(entity =>
        {
            entity.ToTable("studio_videos");
            entity.HasKey(video => video.Id);
            entity.HasIndex(video => video.Stage);
            entity.Property(video => video.Title).HasMaxLength(240);
            entity.Property(video => video.Topic).HasMaxLength(240);
            entity.Property(video => video.Format).HasMaxLength(80);
            entity.Property(video => video.Stage).HasMaxLength(60);
            entity.Property(video => video.StorageFolder).HasMaxLength(240);
            entity.Property(video => video.DriveFileId).HasMaxLength(200);
            entity.Property(video => video.SourceAgentKey).HasMaxLength(120);
            entity.Property(video => video.Platforms).HasColumnType("text[]");
        });

        modelBuilder.Entity<StudioPublicationEntity>(entity =>
        {
            entity.ToTable("studio_publications");
            entity.HasKey(publication => publication.Id);
            entity.HasIndex(publication => publication.Platform);
            entity.Property(publication => publication.Platform).HasMaxLength(80);
            entity.Property(publication => publication.Status).HasMaxLength(40);
            entity.Property(publication => publication.PublishedUrl).HasMaxLength(500);
            entity.HasOne(publication => publication.Video)
                .WithMany()
                .HasForeignKey(publication => publication.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudioScheduleJobEntity>(entity =>
        {
            entity.ToTable("studio_schedule_jobs");
            entity.HasKey(job => job.Id);
            entity.HasIndex(job => job.Type);
            entity.Property(job => job.Name).HasMaxLength(200);
            entity.Property(job => job.Type).HasMaxLength(60);
            entity.Property(job => job.AgentKey).HasMaxLength(120);
            entity.Property(job => job.Status).HasMaxLength(60);
            entity.Property(job => job.Trigger).HasMaxLength(120);
            entity.Property(job => job.QueueMode).HasMaxLength(60);
        });

        modelBuilder.Entity<StudioChatMessageEntity>(entity =>
        {
            entity.ToTable("studio_chat_messages");
            entity.HasKey(message => message.Id);
            entity.HasIndex(message => new { message.AgentKey, message.CreatedAt });
            entity.Property(message => message.AgentKey).HasMaxLength(120);
            entity.Property(message => message.Role).HasMaxLength(40);
        });

        modelBuilder.Entity<StudioAgentRunEntity>(entity =>
        {
            entity.ToTable("studio_agent_runs");
            entity.HasKey(run => run.Id);
            entity.HasIndex(run => new { run.AgentKey, run.QueuedAt });
            entity.Property(run => run.AgentKey).HasMaxLength(120);
            entity.Property(run => run.Title).HasMaxLength(200);
            entity.Property(run => run.Status).HasMaxLength(60);
        });

        modelBuilder.Entity<StudioDriveConfigEntity>(entity =>
        {
            entity.ToTable("studio_drive_configs");
            entity.HasKey(config => config.Id);
            entity.Property(config => config.ClientId).HasMaxLength(300);
            entity.Property(config => config.RootFolderId).HasMaxLength(200);
        });

        modelBuilder.Entity<StudioAgentConnectionEntity>(entity =>
        {
            entity.ToTable("studio_agent_connections");
            entity.HasKey(conn => conn.Id);
            entity.HasIndex(conn => conn.AgentKey).IsUnique();
            entity.Property(conn => conn.AgentKey).HasMaxLength(120);
            entity.Property(conn => conn.ProviderName).HasMaxLength(120);
            entity.Property(conn => conn.ModelName).HasMaxLength(160);
        });

        // Video Pipeline Configurations
        modelBuilder.Entity<VideoPipelineJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CurrentStage).HasConversion<string>();
            entity.HasMany(e => e.Stages).WithOne().HasForeignKey(s => s.JobId);
            entity.HasOne(e => e.Metadata).WithOne().HasForeignKey<VideoMetadata>(m => m.JobId);
        });

        modelBuilder.Entity<PipelineStage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StageType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<VideoMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PlatformPublishJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Platform).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // Decision Layer Configurations
        modelBuilder.Entity<AgentDecision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Outcome).HasConversion<string>();
        });

        modelBuilder.Entity<PromptTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DecisionType).HasConversion<string>();
        });

        modelBuilder.Entity<DecisionValidation>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DecisionCacheEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CacheKey).IsUnique();
        });

        // ==========================
        // V2 Entities Configurations
        // ==========================

        // Brain
        modelBuilder.Entity<BrainState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AgentHealthMap).HasColumnType("jsonb");
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<BrainTickLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TickNumber);
            entity.HasIndex(e => e.StartedAt);
        });

        // Local Memory
        modelBuilder.Entity<AgentLocalMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
            entity.Property(e => e.ConfigJson).HasColumnType("jsonb");
        });

        // Agents
        modelBuilder.Entity<ScriptOutput>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.Property(e => e.Keywords).HasColumnType("jsonb");
            entity.Property(e => e.Hashtags).HasColumnType("jsonb");
            entity.Property(e => e.SuggestedPlatforms).HasColumnType("jsonb");
        });

        modelBuilder.Entity<EditPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.Property(e => e.Segments).HasColumnType("jsonb");
            entity.Property(e => e.Captions).HasColumnType("jsonb");
            entity.Property(e => e.AudioAdjustments).HasColumnType("jsonb");
            entity.Property(e => e.Transitions).HasColumnType("jsonb");
            entity.Property(e => e.ColorGrading).HasColumnType("jsonb");
            entity.Property(e => e.FFmpegCommands).HasColumnType("jsonb");
        });

        modelBuilder.Entity<ShortClip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<ShortEditPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShortClipId);
            entity.Property(e => e.HookOverlay).HasColumnType("jsonb");
            entity.Property(e => e.Captions).HasColumnType("jsonb");
            entity.Property(e => e.MusicTrack).HasColumnType("jsonb");
            entity.Property(e => e.EmojiOverlays).HasColumnType("jsonb");
            entity.Property(e => e.Watermark).HasColumnType("jsonb");
            entity.Property(e => e.FFmpegCommands).HasColumnType("jsonb");
        });

        modelBuilder.Entity<TrendResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DiscoveredAt);
            entity.Property(e => e.Topics).HasColumnType("jsonb");
            entity.Property(e => e.PlannedUploads).HasColumnType("jsonb");
            entity.Property(e => e.TopKeywords).HasColumnType("jsonb");
            entity.Property(e => e.TopHashtags).HasColumnType("jsonb");
        });

        modelBuilder.Entity<UploadPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Keywords).HasColumnType("jsonb");
            entity.Property(e => e.Hashtags).HasColumnType("jsonb");
            entity.Property(e => e.TargetPlatforms).HasColumnType("jsonb");
        });

        modelBuilder.Entity<AnalyticsReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReportDate);
            entity.Property(e => e.TopPerformingVideos).HasColumnType("jsonb");
            entity.Property(e => e.WorstPerformingVideos).HasColumnType("jsonb");
            entity.Property(e => e.DetectedPatterns).HasColumnType("jsonb");
            entity.Property(e => e.Recommendations).HasColumnType("jsonb");
        });

        // Errors
        modelBuilder.Entity<DeadLetterEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.Property(e => e.AllErrors).HasColumnType("jsonb");
        });

        modelBuilder.Entity<CircuitBreakerState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });

        modelBuilder.Entity<RetryPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
            entity.Property(e => e.BackoffSeconds).HasColumnType("jsonb");
            entity.Property(e => e.RetryOnExceptions).HasColumnType("jsonb");
            entity.Property(e => e.SkipOnExceptions).HasColumnType("jsonb");
        });

        // Decisions Audit
        modelBuilder.Entity<DecisionAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DecisionId);
        });
        modelBuilder.Entity<YouTubeCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });

        modelBuilder.Entity<YouTubeUploadResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.YouTubeVideoId);
        });
        modelBuilder.Entity<TikTokCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });

        modelBuilder.Entity<TikTokUploadResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TikTokVideoId);
        });
        modelBuilder.Entity<InstagramCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });

        modelBuilder.Entity<InstagramUploadResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InstagramMediaId);
        });
        modelBuilder.Entity<FacebookCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });

        modelBuilder.Entity<LinkedInCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentKey).IsUnique();
        });
    }
}
