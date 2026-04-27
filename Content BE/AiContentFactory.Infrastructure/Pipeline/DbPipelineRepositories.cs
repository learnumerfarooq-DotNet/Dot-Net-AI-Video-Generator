using AiContentFactory.Application.Pipeline;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class DbVideoPipelineRepository : IVideoPipelineRepository
{
    private readonly StudioDbContext _dbContext;

    public DbVideoPipelineRepository(StudioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VideoPipelineJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.VideoPipelineJobs
            .Include(j => j.Stages)
            .Include(j => j.Metadata)
            .Include(j => j.PublishJobs)
            .FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task<IReadOnlyList<VideoPipelineJob>> GetActiveJobsAsync(CancellationToken ct = default)
    {
        return await _dbContext.VideoPipelineJobs
            .Where(j => j.Status != PipelineStatus.AnalyticsCollected && j.Status != PipelineStatus.Failed)
            .Include(j => j.Stages)
            .ToListAsync(ct);
    }

    public async Task AddAsync(VideoPipelineJob job, CancellationToken ct = default)
    {
        await _dbContext.VideoPipelineJobs.AddAsync(job, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VideoPipelineJob job, CancellationToken ct = default)
    {
        _dbContext.VideoPipelineJobs.Update(job);
        await _dbContext.SaveChangesAsync(ct);
    }
}

public sealed class DbPlatformPublishRepository : IPlatformPublishRepository
{
    private readonly StudioDbContext _dbContext;

    public DbPlatformPublishRepository(StudioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlatformPublishJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.PlatformPublishJobs
            .FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task AddAsync(PlatformPublishJob job, CancellationToken ct = default)
    {
        await _dbContext.PlatformPublishJobs.AddAsync(job, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlatformPublishJob job, CancellationToken ct = default)
    {
        _dbContext.PlatformPublishJobs.Update(job);
        await _dbContext.SaveChangesAsync(ct);
    }
}
