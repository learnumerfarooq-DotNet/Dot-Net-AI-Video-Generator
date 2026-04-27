using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class UploadAgent : IUploadAgent
{
    private readonly UploadMetadataGenerator _metadataGenerator;
    private readonly StudioDbContext _dbContext;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ILogger<UploadAgent> _logger;

    public UploadAgent(
        UploadMetadataGenerator metadataGenerator,
        StudioDbContext dbContext,
        IPipelineOrchestrator orchestrator,
        ILogger<UploadAgent> logger)
    {
        _metadataGenerator = metadataGenerator;
        _dbContext = dbContext;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<UploadPackage> PrepareUploadAsync(Guid jobId, CancellationToken ct = default)
    {
        _logger.LogInformation("Preparing upload package for job {JobId}", jobId);
        
        var job = await _dbContext.VideoPipelineJobs.FindAsync(new object[] { jobId }, ct) ?? throw new Exception("Job not found");
        var script = await _dbContext.ScriptOutputs.FirstOrDefaultAsync(s => s.JobId == jobId, ct) ?? throw new Exception("Script not found");

        var package = new UploadPackage
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            VideoType = job.Metadata?.Resolution.Contains("1080x1920") == true ? "short" : "long",
            SourceDriveFileId = job.DriveFileId,
            Status = UploadPackageStatus.Preparing,
            CreatedAt = DateTimeOffset.UtcNow,
            TargetPlatforms = new List<string> { "YouTube", "TikTok" }
        };

        _dbContext.UploadPackages.Add(package);
        await _dbContext.SaveChangesAsync(ct);

        return await GenerateMetadataAsync(package.Id, ct);
    }

    public async Task<UploadPackage> GenerateMetadataAsync(Guid packageId, CancellationToken ct = default)
    {
        var package = await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct) ?? throw new Exception("Package not found");
        var script = await _dbContext.ScriptOutputs.FirstOrDefaultAsync(s => s.JobId == package.JobId, ct) ?? throw new Exception("Script not found");

        var metadata = await _metadataGenerator.GenerateMetadataAsync(script, package.TargetPlatforms.First(), package.JobId, ct);
        
        package.Title = metadata.Title;
        package.Description = metadata.Description;
        package.Keywords = metadata.Keywords;
        package.Hashtags = metadata.Hashtags;
        package.Category = metadata.Category;
        package.Privacy = metadata.Privacy;
        package.Status = UploadPackageStatus.Ready;

        await _dbContext.SaveChangesAsync(ct);
        return package;
    }

    public async Task AssignToScheduleSlotAsync(Guid packageId, Guid slotId, CancellationToken ct = default)
    {
        var package = await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct) ?? throw new Exception("Package not found");
        package.ScheduleSlotId = slotId;
        // In a real app, we'd look up the slot time here
        package.ScheduledTime = DateTimeOffset.UtcNow.AddHours(2);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<PlatformPublishJob>> CreatePublishJobsAsync(Guid packageId, CancellationToken ct = default)
    {
        var package = await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct) ?? throw new Exception("Package not found");
        
        var jobs = new List<PlatformPublishJob>();
        foreach (var platformName in package.TargetPlatforms)
        {
            if (Enum.TryParse<PlatformType>(platformName, true, out var platform))
            {
                var job = PlatformPublishJob.Create(package.JobId, platform);
                jobs.Add(job);
            }
        }

        package.PublishJobs = jobs;
        package.Status = UploadPackageStatus.Publishing;
        await _dbContext.SaveChangesAsync(ct);
        
        return jobs;
    }

    public async Task ExecuteUploadAsync(Guid packageId, CancellationToken ct = default)
    {
        var package = await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct) ?? throw new Exception("Package not found");
        
        _logger.LogInformation("Executing multi-platform upload for package {PackageId}", packageId);
        
        // This would trigger the actual publishers
        package.Status = UploadPackageStatus.Published;
        await _dbContext.SaveChangesAsync(ct);

        await _orchestrator.TransitionStageAsync(package.JobId, PipelineStageType.PlatformPublishing, ct);
    }

    public async Task<UploadPackage?> GetPackageAsync(Guid packageId, CancellationToken ct = default)
    {
        return await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct);
    }

    public async Task<List<UploadPackage>> GetPendingPackagesAsync(CancellationToken ct = default)
    {
        return await _dbContext.UploadPackages
            .Where(p => p.Status == UploadPackageStatus.Ready)
            .ToListAsync(ct);
    }

    public async Task ApprovePackageAsync(Guid packageId, CancellationToken ct = default)
    {
        var package = await _dbContext.UploadPackages.FindAsync(new object[] { packageId }, ct) ?? throw new Exception("Package not found");
        package.ApprovalRequired = false;
        await _dbContext.SaveChangesAsync(ct);
    }
}
