using AiContentFactory.Domain.Pipeline;

namespace AiContentFactory.Application.Pipeline;

public interface IPipelineOrchestrator
{
    Task<VideoPipelineJob> StartPipelineAsync(string driveFileId, string fileName, CancellationToken ct = default);
    Task TransitionStageAsync(Guid jobId, PipelineStageType stage, CancellationToken ct = default);
    Task HandleFailureAsync(Guid jobId, string errorMessage, CancellationToken ct = default);
    Task<VideoPipelineJob?> GetJobAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<VideoPipelineJob>> GetActiveJobsAsync(CancellationToken ct = default);
}

public interface IDriveFolderWatcher
{
    Task<IReadOnlyList<DriveFileChange>> PollForChangesAsync(string folderPath, CancellationToken ct = default);
}

public record DriveFileChange(string FileId, string FileName, DateTimeOffset ModifiedAt, ChangeType Type);

public enum ChangeType
{
    Created,
    Modified,
    Deleted
}

public interface IAgentDispatcher
{
    Task DispatchAsync(Guid jobId, PipelineStageType stage, object payload, CancellationToken ct = default);
}
