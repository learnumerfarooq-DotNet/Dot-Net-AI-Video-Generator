namespace AiContentFactory.Domain.Events;

public record BrainTickPayload(long TickNumber, int ActiveJobs, int PendingJobs, int FailedJobs, long DurationMs);
public record BrainStatusPayload(string Status, string PreviousStatus);
public record MemorySyncedPayload(string Version, int FolderCount);
public record CircuitBreakerPayload(string AgentKey, string State, int FailureCount);
public record BrainPausePayload(bool IsPaused, string Reason);

public record JobStartedPayload(Guid JobId, string FileName, string DriveFileId);
public record StageCompletedPayload(Guid JobId, string StageName, double Progress);
public record ProgressUpdatedPayload(Guid JobId, string Stage, int Percent, string Message);
public record JobCompletedPayload(Guid JobId, string FileName, long DurationMs);
public record JobFailedPayload(Guid JobId, string Error, int RetryCount, bool IsDeadLettered);
public record JobRetryingPayload(Guid JobId, int Attempt, DateTime NextRetryAt);

public record AgentDispatchedPayload(string AgentKey, Guid JobId, string Queue);
public record AgentRunStartedPayload(string AgentKey, Guid RunId, Guid JobId);
public record AgentRunCompletedPayload(string AgentKey, Guid RunId, string Status, long DurationMs);
public record AgentHealthPayload(string AgentKey, string Status, string Reason);
public record AgentChatResponsePayload(string AgentKey, string Message, string Role);
public record AgentChatStreamPayload(string AgentKey, string Chunk, string Type);

public record ScriptGeneratedPayload(Guid JobId, string Title, double Confidence);
public record VideoEditedPayload(Guid JobId, string OutputFileId, long DurationSeconds);
public record ShortsCreatedPayload(Guid JobId, int ClipCount, long TotalDurationSeconds);
public record ShortEditCompletedPayload(Guid JobId, string ClipId, string OutputFileId);
public record UploadPackagePayload(string PackageId, Guid JobId, int PlatformCount);

public record UploadStartedPayload(string PackageId, string Platform);
public record UploadProgressPayload(string PackageId, string Platform, int Percent);
public record UploadCompletedPayload(string PackageId, string Platform, string VideoId, string Url);
public record UploadFailedPayload(string PackageId, string Platform, string Error);

public record TrendDiscoveryPayload(int TopicCount, string[] TopKeywords);
public record AnalyticsReportPayload(string ReportId, int VideosAnalyzed);
public record MemorySuggestionPayload(Guid Id, string Scope, string AgentKey);
public record DriveFileDetectedPayload(string FileId, string FileName, string Folder);
