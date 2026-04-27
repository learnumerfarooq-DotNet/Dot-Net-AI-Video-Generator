using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Pipeline;

namespace AiContentFactory.Application.Agents;

public interface IUploadAgent
{
    Task<UploadPackage> PrepareUploadAsync(Guid jobId, CancellationToken ct = default);
    Task<UploadPackage> GenerateMetadataAsync(Guid packageId, CancellationToken ct = default);
    Task AssignToScheduleSlotAsync(Guid packageId, Guid slotId, CancellationToken ct = default);
    Task<List<PlatformPublishJob>> CreatePublishJobsAsync(Guid packageId, CancellationToken ct = default);
    Task ExecuteUploadAsync(Guid packageId, CancellationToken ct = default);
    Task<UploadPackage?> GetPackageAsync(Guid packageId, CancellationToken ct = default);
    Task<List<UploadPackage>> GetPendingPackagesAsync(CancellationToken ct = default);
    Task ApprovePackageAsync(Guid packageId, CancellationToken ct = default);
}
