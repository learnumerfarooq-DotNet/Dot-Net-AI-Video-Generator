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
    }
}
