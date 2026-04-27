using AiContentFactory.Application.Configuration;
using AiContentFactory.Application.AI;
using AiContentFactory.Application.Agents;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Application.Errors;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Application.Brain;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Publishing;
using AiContentFactory.Infrastructure.Agents;
using AiContentFactory.Infrastructure.Backlog;
using AiContentFactory.Infrastructure.Memory;
using AiContentFactory.Infrastructure.Publishing.YouTube;
using AiContentFactory.Infrastructure.Publishing.TikTok;
using AiContentFactory.Infrastructure.Publishing.Instagram;
using AiContentFactory.Infrastructure.Publishing.Facebook;
using AiContentFactory.Infrastructure.Publishing.LinkedIn;
using AiContentFactory.Infrastructure.Persistence;
using AiContentFactory.Infrastructure.Providers;
using AiContentFactory.Infrastructure.Scheduler;
using AiContentFactory.Infrastructure.Security;
using AiContentFactory.Infrastructure.Storage;
using AiContentFactory.Infrastructure.Tools;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Infrastructure.Processing;
using AiContentFactory.Infrastructure.Pipeline;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Infrastructure.Decisions;
using AiContentFactory.Infrastructure.AI;
using AiContentFactory.Infrastructure.Shorts;
using AiContentFactory.Infrastructure.Trends;
using AiContentFactory.Infrastructure.Publishing;
using AiContentFactory.Infrastructure.Analytics;
using AiContentFactory.Infrastructure.Errors;
using AiContentFactory.Infrastructure.Brain;
using AiContentFactory.Application.Brain;
using AiContentFactory.Application.Memory;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;


namespace AiContentFactory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ContentFactoryOptions>(configuration.GetSection(ContentFactoryOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Missing Postgres connection string.");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            return dataSourceBuilder.Build();
        });

        services.AddDbContext<StudioDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
            
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IMaskingService, MaskingService>();
        services.AddHostedService<DriveSyncService>();
        services.AddHostedService<EmbeddingSyncService>();
        services.AddHostedService<MemoryCleanupService>();
        services.AddScoped<IStudioWorkspaceStore, StudioWorkspaceStore>();
        services.AddSingleton<IJsonFileStore, JsonFileStore>();
        services.AddScoped<IMemoryRepository, DbMemoryRepository>();
        services.AddScoped<IBacklogRepository, DbBacklogRepository>();
        services.AddSingleton<IProviderConfigurationRepository, JsonProviderConfigurationRepository>();
        services.AddSingleton<IProviderCredentialRepository, JsonProviderCredentialRepository>();
        services.AddSingleton<IProviderRequirementCatalog, ProviderRequirementCatalog>();
        services.AddSingleton<ITextGenerationProvider, TemplateTextGenerationProvider>();
        services.AddSingleton<IUploadExecutionProvider, DryRunUploadExecutionProvider>();
        services.AddHttpClient<IChatProvider, OpenRouterChatProvider>();
        services.AddScoped<IScriptGenAgent, ScriptGenExecutionAgent>();
        services.AddScoped<IEditAgent, EditExecutionAgent>();
        services.AddScoped<UploadMetadataGenerator>();
        services.AddScoped<IUploadAgent, UploadAgent>();
        services.AddHttpClient<IGoogleDriveService, GoogleDriveService>();
        services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingProvider>();
        
        // Agent Tools
        services.AddScoped<IStudioTool, TrendSearchTool>();
        services.AddScoped<IStudioTool, ScriptDraftTool>();
        services.AddScoped<IStudioTool, ScheduleTaskTool>();
        services.AddScoped<StudioToolRegistry>();

        // NEW: FFmpeg and Processing
        services.Configure<FFmpegOptions>(configuration.GetSection("FFmpeg"));
        services.AddSingleton<IFFmpegService, FFmpegService>();
        services.AddSingleton<IVideoMetadataExtractor, VideoMetadataExtractor>();
        services.AddSingleton<ITempStorageManager, TempStorageManager>();

        // NEW: Pipeline Orchestration
        services.AddScoped<IPipelineOrchestrator, PipelineOrchestrator>();
        services.AddScoped<IDriveFolderWatcher, DriveFolderWatcher>();
        services.AddScoped<IAgentDispatcher, AgentDispatcher>();
        services.AddScoped<RawPipelineHandler>();
        services.AddHostedService<DrivePollingBackgroundService>();

        // NEW: Hangfire Job Queue
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration.GetConnectionString("Postgres"))));

        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "ffmpeg", "ai", "upload", "default" };
            options.WorkerCount = 4;
        });

        // NEW: Pipeline repositories
        services.AddScoped<IVideoPipelineRepository, DbVideoPipelineRepository>();
        services.AddScoped<IPlatformPublishRepository, DbPlatformPublishRepository>();

        // NEW: Decision Layer
        services.Configure<StructuredChatOptions>(configuration.GetSection("OpenRouter"));
        services.AddHttpClient<IStructuredChatProvider, StructuredChatProvider>();
        services.AddScoped<IDecisionEngine, DecisionEngine>();
        services.AddScoped<IDecisionValidator, DecisionValidator>();
        services.AddScoped<IAgentDecisionFacade, AgentDecisionFacade>();
        services.AddScoped<IDecisionCache, DecisionCache>();
        services.AddScoped<PromptVersionManager>();
        services.AddScoped<PromptTemplateSeeder>();
        
        // NEW: Shorts Pipeline
        services.AddScoped<AspectRatioConverter>();
        services.AddScoped<ShortDurationEnforcer>();
        services.AddScoped<SegmentScorer>();
        services.AddScoped<IShortsAgent, ShortExecutionAgent>();
        services.AddScoped<IShortEditAgent, ShortEditExecutionAgent>();
        services.AddScoped<CaptionRenderer>();
        services.AddScoped<MusicOverlayService>();
        
        // NEW: Trend Discovery & Scheduling
        services.Configure<TrendOptions>(configuration.GetSection("Trends"));
        services.AddScoped<SiteScraper>();
        services.AddScoped<TrendAnalyzer>();
        services.AddScoped<TrendScheduler>();
        services.AddScoped<ITrendAgent, TrendAgent>();
        services.AddScoped<TrendDiscoveryJob>();
        
        // NEW: Publishing
        services.Configure<YouTubeOptions>(configuration.GetSection("YouTube"));
        services.AddScoped<YouTubeOAuthManager>();
        services.AddScoped<YouTubeUploadService>();
        services.AddScoped<YouTubeAnalyticsService>();
        services.AddScoped<IPlatformPublisher, YouTubePublisher>();

        services.Configure<TikTokOptions>(configuration.GetSection("TikTok"));
        services.AddScoped<TikTokOAuthManager>();
        services.AddScoped<IPlatformPublisher, TikTokPublisher>();

        services.Configure<InstagramOptions>(configuration.GetSection("Instagram"));
        services.AddScoped<InstagramOAuthManager>();
        services.AddScoped<IPlatformPublisher, InstagramPublisher>();

        services.Configure<FacebookOptions>(configuration.GetSection("Facebook"));
        services.AddScoped<IPlatformPublisher, FacebookPublisher>();

        services.Configure<LinkedInOptions>(configuration.GetSection("LinkedIn"));
        services.AddScoped<IPlatformPublisher, LinkedInPublisher>();
        
        services.AddScoped<StreamingUploadService>();
        
        // NEW: Analytics & Feedback Loop
        services.Configure<AnalyticsOptions>(configuration.GetSection("Analytics"));
        services.AddScoped<PlatformStatsCollector>();
        services.AddScoped<ContentScoreCalculator>();
        services.AddScoped<AnalyticsCollector>();
        services.AddScoped<ViralPatternDetector>();
        services.AddScoped<FeedbackLoopEngine>();
        services.AddScoped<IAnalyticsAgent, AnalyticsAgent>();
        services.AddScoped<DailyAnalyticsJob>();
        
        // NEW: Error Handling & Resilience
        services.Configure<ErrorHandlingOptions>(configuration.GetSection("ErrorHandling"));
        services.AddSingleton<RetryManager>();
        services.AddSingleton<CircuitBreakerManager>();
        services.AddScoped<FailureMonitor>();
        services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
        
        // NEW: Brain Orchestrator
        services.Configure<BrainOptions>(configuration.GetSection(BrainOptions.SectionName));
        services.AddScoped<IBrainOrchestrator, MainBrainService>();
        services.AddScoped<MainBrainJob>();

        // NEW: Global Memory System
        services.AddScoped<IGlobalMemoryService, GlobalMemoryService>();
        services.AddScoped<GlobalMemorySyncJob>();
        
        // NEW: Local Memory System
        services.AddScoped<ILocalMemoryService, LocalMemoryService>();
        services.AddScoped<LocalMemoryDriveSync>();
        
        return services;
    }
}
