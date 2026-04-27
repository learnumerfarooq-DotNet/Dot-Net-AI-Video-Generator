using AiContentFactory.Domain.Pipeline;

namespace AiContentFactory.Application.Pipeline;

public interface IVideoPipelineRepository
{
    Task<VideoPipelineJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<VideoPipelineJob>> GetActiveJobsAsync(CancellationToken ct = default);
    Task AddAsync(VideoPipelineJob job, CancellationToken ct = default);
    Task UpdateAsync(VideoPipelineJob job, CancellationToken ct = default);
}

public interface IPlatformPublishRepository
{
    Task<PlatformPublishJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PlatformPublishJob job, CancellationToken ct = default);
    Task UpdateAsync(PlatformPublishJob job, CancellationToken ct = default);
}
