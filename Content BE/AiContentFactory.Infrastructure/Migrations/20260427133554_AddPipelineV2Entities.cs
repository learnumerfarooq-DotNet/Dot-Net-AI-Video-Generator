using System;
using System.Collections.Generic;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Analytics;
using AiContentFactory.Domain.Brain;
using AiContentFactory.Domain.Trends;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiContentFactory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineV2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoPipelineJobId",
                table: "VideoAnalytics",
                newName: "VideoId");

            migrationBuilder.Sql("ALTER TABLE \"ViralPatterns\" ALTER COLUMN \"AffectedVideos\" TYPE uuid[] USING \"AffectedVideos\"::uuid[];");

            migrationBuilder.AddColumn<double>(
                name: "CTR",
                table: "VideoAnalytics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WatchTime",
                table: "VideoAnalytics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadPackageId",
                table: "PlatformPublishJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgentLocalMemories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    AgentDisplayName = table.Column<string>(type: "text", nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RunCount = table.Column<long>(type: "bigint", nullable: false),
                    SuccessCount = table.Column<long>(type: "bigint", nullable: false),
                    FailureCount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentLocalMemories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Period = table.Column<string>(type: "text", nullable: false),
                    TotalVideosAnalyzed = table.Column<int>(type: "integer", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    TotalLikes = table.Column<long>(type: "bigint", nullable: false),
                    TotalComments = table.Column<long>(type: "bigint", nullable: false),
                    TotalShares = table.Column<long>(type: "bigint", nullable: false),
                    AverageCTR = table.Column<double>(type: "double precision", nullable: false),
                    AverageWatchTime = table.Column<double>(type: "double precision", nullable: false),
                    AverageEngagement = table.Column<double>(type: "double precision", nullable: false),
                    TopPerformingVideos = table.Column<string>(type: "jsonb", nullable: false),
                    WorstPerformingVideos = table.Column<string>(type: "jsonb", nullable: false),
                    DetectedPatterns = table.Column<List<ViralPattern>>(type: "jsonb", nullable: false),
                    Recommendations = table.Column<string>(type: "jsonb", nullable: false),
                    DriveFileId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrainStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentTickNumber = table.Column<long>(type: "bigint", nullable: false),
                    LastTickAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastGlobalMemorySync = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActiveJobCount = table.Column<int>(type: "integer", nullable: false),
                    PendingJobCount = table.Column<int>(type: "integer", nullable: false),
                    FailedJobCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedJobCount = table.Column<int>(type: "integer", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AgentHealthMap = table.Column<Dictionary<string, AgentHealthStatus>>(type: "jsonb", nullable: false),
                    GlobalMemoryVersion = table.Column<string>(type: "text", nullable: false),
                    IsCircuitBreakerOpen = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrainStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrainTickLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TickNumber = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    JobsDispatched = table.Column<int>(type: "integer", nullable: false),
                    JobsCompleted = table.Column<int>(type: "integer", nullable: false),
                    JobsFailed = table.Column<int>(type: "integer", nullable: false),
                    GlobalMemoryRead = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrainTickLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircuitBreakerStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    Threshold = table.Column<int>(type: "integer", nullable: false),
                    PauseMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastFailureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircuitBreakerStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeadLetterEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    OriginalError = table.Column<string>(type: "text", nullable: false),
                    AllErrors = table.Column<string>(type: "jsonb", nullable: false),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    FirstFailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastFailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsResolvable = table.Column<bool>(type: "boolean", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecisionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    DecisionType = table.Column<int>(type: "integer", nullable: false),
                    InputContextHash = table.Column<string>(type: "text", nullable: false),
                    RawResponse = table.Column<string>(type: "text", nullable: false),
                    ValidatedResponse = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScriptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Segments = table.Column<List<EditSegment>>(type: "jsonb", nullable: false),
                    Captions = table.Column<List<EditCaption>>(type: "jsonb", nullable: false),
                    AudioAdjustments = table.Column<List<AudioAdjustment>>(type: "jsonb", nullable: false),
                    Transitions = table.Column<List<TransitionPlan>>(type: "jsonb", nullable: false),
                    ColorGrading = table.Column<ColorGradingConfig>(type: "jsonb", nullable: true),
                    OutputFormat = table.Column<string>(type: "text", nullable: false),
                    OutputCodec = table.Column<string>(type: "text", nullable: false),
                    OutputResolution = table.Column<string>(type: "text", nullable: false),
                    OutputFps = table.Column<int>(type: "integer", nullable: false),
                    EstimatedOutputSize = table.Column<long>(type: "bigint", nullable: false),
                    FFmpegCommands = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InputDriveFileId = table.Column<string>(type: "text", nullable: false),
                    OutputDriveFileId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacebookUploadResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPublishJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacebookVideoId = table.Column<string>(type: "text", nullable: false),
                    FacebookUrl = table.Column<string>(type: "text", nullable: false),
                    PageId = table.Column<string>(type: "text", nullable: false),
                    PageName = table.Column<string>(type: "text", nullable: false),
                    UploadStatus = table.Column<string>(type: "text", nullable: false),
                    Privacy = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ScheduledPublishTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacebookUploadResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramUploadResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPublishJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstagramMediaId = table.Column<string>(type: "text", nullable: false),
                    InstagramUrl = table.Column<string>(type: "text", nullable: false),
                    AccountId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    MediaType = table.Column<string>(type: "text", nullable: false),
                    UploadStatus = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: false),
                    Hashtags = table.Column<List<string>>(type: "text[]", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramUploadResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkedInUploadResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPublishJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedInPostUrn = table.Column<string>(type: "text", nullable: false),
                    LinkedInUrl = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    AuthorUrn = table.Column<string>(type: "text", nullable: false),
                    UploadStatus = table.Column<string>(type: "text", nullable: false),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    Commentary = table.Column<string>(type: "text", nullable: false),
                    AssetUrn = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedInUploadResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetryPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentKey = table.Column<string>(type: "text", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    BackoffSeconds = table.Column<string>(type: "jsonb", nullable: false),
                    BackoffType = table.Column<string>(type: "text", nullable: false),
                    RetryOnExceptions = table.Column<string>(type: "jsonb", nullable: false),
                    SkipOnExceptions = table.Column<string>(type: "jsonb", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetryPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapeResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteUrl = table.Column<string>(type: "text", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    TopicsFound = table.Column<int>(type: "integer", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: false),
                    ScrapedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    ResponseCode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapeResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScriptOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Hook = table.Column<string>(type: "text", nullable: false),
                    Introduction = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    CallToAction = table.Column<string>(type: "text", nullable: false),
                    Keywords = table.Column<string>(type: "jsonb", nullable: false),
                    Hashtags = table.Column<string>(type: "jsonb", nullable: false),
                    SuggestedPlatforms = table.Column<string>(type: "jsonb", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    ToneUsed = table.Column<string>(type: "text", nullable: false),
                    StyleUsed = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    DriveFileId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptOutputs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortClips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentVideoFileId = table.Column<string>(type: "text", nullable: false),
                    ClipNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Hook = table.Column<string>(type: "text", nullable: false),
                    Rationale = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<double>(type: "double precision", nullable: false),
                    EndTime = table.Column<double>(type: "double precision", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    AspectRatio = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OutputFileName = table.Column<string>(type: "text", nullable: false),
                    DriveFileId = table.Column<string>(type: "text", nullable: true),
                    EngagementScore = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortClips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortEditPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortClipId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    HookOverlay = table.Column<HookOverlayConfig>(type: "jsonb", nullable: true),
                    Captions = table.Column<List<ShortCaption>>(type: "jsonb", nullable: false),
                    MusicTrack = table.Column<MusicTrackConfig>(type: "jsonb", nullable: true),
                    EmojiOverlays = table.Column<List<EmojiOverlay>>(type: "jsonb", nullable: false),
                    TransitionIn = table.Column<string>(type: "text", nullable: false),
                    TransitionOut = table.Column<string>(type: "text", nullable: false),
                    Watermark = table.Column<WatermarkConfig>(type: "jsonb", nullable: true),
                    OutputDriveFileId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FFmpegCommands = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortEditPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TikTokUploadResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPublishJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    TikTokVideoId = table.Column<string>(type: "text", nullable: false),
                    TikTokUrl = table.Column<string>(type: "text", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    UploadStatus = table.Column<string>(type: "text", nullable: false),
                    PrivacyLevel = table.Column<string>(type: "text", nullable: false),
                    AllowComments = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDuet = table.Column<bool>(type: "boolean", nullable: false),
                    AllowStitch = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TikTokUploadResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrendResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Topics = table.Column<List<DiscoveredTopic>>(type: "jsonb", nullable: false),
                    PlannedUploads = table.Column<List<PlannedUpload>>(type: "jsonb", nullable: false),
                    AnalysisSummary = table.Column<string>(type: "text", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalSitesScraped = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulScrapes = table.Column<int>(type: "integer", nullable: false),
                    FailedScrapes = table.Column<int>(type: "integer", nullable: false),
                    UsedOpenRouterFallback = table.Column<bool>(type: "boolean", nullable: false),
                    TopKeywords = table.Column<string>(type: "jsonb", nullable: false),
                    TopHashtags = table.Column<string>(type: "jsonb", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    DriveFileId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoType = table.Column<string>(type: "text", nullable: false),
                    SourceDriveFileId = table.Column<string>(type: "text", nullable: false),
                    SourceFolder = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Keywords = table.Column<string>(type: "jsonb", nullable: false),
                    Hashtags = table.Column<string>(type: "jsonb", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Privacy = table.Column<string>(type: "text", nullable: false),
                    ScheduledTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TargetPlatforms = table.Column<string>(type: "jsonb", nullable: false),
                    ThumbnailDriveFileId = table.Column<string>(type: "text", nullable: true),
                    ThumbnailText = table.Column<string>(type: "text", nullable: true),
                    ScheduleSlotId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrendKeyword = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    ApprovalRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Fps = table.Column<double>(type: "double precision", nullable: false),
                    Codec = table.Column<string>(type: "text", nullable: false),
                    Bitrate = table.Column<long>(type: "bigint", nullable: false),
                    AudioChannels = table.Column<int>(type: "integer", nullable: false),
                    AudioSampleRate = table.Column<int>(type: "integer", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SceneChanges = table.Column<List<double>>(type: "double precision[]", nullable: false),
                    AverageVolume = table.Column<double>(type: "double precision", nullable: false),
                    PeakVolume = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YouTubeUploadResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPublishJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    YouTubeVideoId = table.Column<string>(type: "text", nullable: false),
                    YouTubeUrl = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<string>(type: "text", nullable: false),
                    ChannelTitle = table.Column<string>(type: "text", nullable: false),
                    UploadStatus = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: false),
                    PrivacyStatus = table.Column<string>(type: "text", nullable: false),
                    IsShort = table.Column<bool>(type: "boolean", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeUploadResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPublishJobs_UploadPackageId",
                table: "PlatformPublishJobs",
                column: "UploadPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentLocalMemories_AgentKey",
                table: "AgentLocalMemories",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsReports_ReportDate",
                table: "AnalyticsReports",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_BrainStates_Status",
                table: "BrainStates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BrainTickLogs_StartedAt",
                table: "BrainTickLogs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrainTickLogs_TickNumber",
                table: "BrainTickLogs",
                column: "TickNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CircuitBreakerStates_AgentKey",
                table: "CircuitBreakerStates",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterEntries_JobId",
                table: "DeadLetterEntries",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionAuditLogs_DecisionId",
                table: "DecisionAuditLogs",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_EditPlans_JobId",
                table: "EditPlans",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_RetryPolicies_AgentKey",
                table: "RetryPolicies",
                column: "AgentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScriptOutputs_JobId",
                table: "ScriptOutputs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ShortClips_JobId",
                table: "ShortClips",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ShortClips_Status",
                table: "ShortClips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShortEditPlans_ShortClipId",
                table: "ShortEditPlans",
                column: "ShortClipId");

            migrationBuilder.CreateIndex(
                name: "IX_TrendResults_DiscoveredAt",
                table: "TrendResults",
                column: "DiscoveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_UploadPackages_JobId",
                table: "UploadPackages",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadPackages_Status",
                table: "UploadPackages",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformPublishJobs_UploadPackages_UploadPackageId",
                table: "PlatformPublishJobs",
                column: "UploadPackageId",
                principalTable: "UploadPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformPublishJobs_UploadPackages_UploadPackageId",
                table: "PlatformPublishJobs");

            migrationBuilder.DropTable(
                name: "AgentLocalMemories");

            migrationBuilder.DropTable(
                name: "AnalyticsReports");

            migrationBuilder.DropTable(
                name: "BrainStates");

            migrationBuilder.DropTable(
                name: "BrainTickLogs");

            migrationBuilder.DropTable(
                name: "CircuitBreakerStates");

            migrationBuilder.DropTable(
                name: "DeadLetterEntries");

            migrationBuilder.DropTable(
                name: "DecisionAuditLogs");

            migrationBuilder.DropTable(
                name: "EditPlans");

            migrationBuilder.DropTable(
                name: "FacebookUploadResults");

            migrationBuilder.DropTable(
                name: "InstagramUploadResults");

            migrationBuilder.DropTable(
                name: "LinkedInUploadResults");

            migrationBuilder.DropTable(
                name: "RetryPolicies");

            migrationBuilder.DropTable(
                name: "ScrapeResults");

            migrationBuilder.DropTable(
                name: "ScriptOutputs");

            migrationBuilder.DropTable(
                name: "ShortClips");

            migrationBuilder.DropTable(
                name: "ShortEditPlans");

            migrationBuilder.DropTable(
                name: "TikTokUploadResults");

            migrationBuilder.DropTable(
                name: "TrendResults");

            migrationBuilder.DropTable(
                name: "UploadPackages");

            migrationBuilder.DropTable(
                name: "VideoAnalysisResults");

            migrationBuilder.DropTable(
                name: "YouTubeUploadResults");

            migrationBuilder.DropIndex(
                name: "IX_PlatformPublishJobs_UploadPackageId",
                table: "PlatformPublishJobs");

            migrationBuilder.DropColumn(
                name: "CTR",
                table: "VideoAnalytics");

            migrationBuilder.DropColumn(
                name: "WatchTime",
                table: "VideoAnalytics");

            migrationBuilder.DropColumn(
                name: "UploadPackageId",
                table: "PlatformPublishJobs");

            migrationBuilder.RenameColumn(
                name: "VideoId",
                table: "VideoAnalytics",
                newName: "VideoPipelineJobId");

            migrationBuilder.Sql("ALTER TABLE \"ViralPatterns\" ALTER COLUMN \"AffectedVideos\" TYPE text[] USING \"AffectedVideos\"::text[];");
        }
    }
}
