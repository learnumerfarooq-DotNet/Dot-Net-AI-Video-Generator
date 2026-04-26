using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class EmbeddingSyncService(
    IServiceProvider serviceProvider,
    ILogger<EmbeddingSyncService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Embedding Sync Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncEmbeddingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while syncing embeddings.");
            }

            // Sync every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task SyncEmbeddingsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        // 1. Process Global Memories
        var pendingGlobal = await dbContext.GlobalMemories
            .Where(m => m.Embedding == null)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var memory in pendingGlobal)
        {
            logger.LogInformation("Generating embedding for global memory: {Title}", memory.Title);
            var vector = await embeddingService.GenerateEmbeddingAsync(memory.Content, cancellationToken);
            if (vector.Length > 0)
            {
                memory.Embedding = vector;
            }
        }

        // 2. Process Agent Memories
        var pendingAgent = await dbContext.AgentMemories
            .Where(m => m.Embedding == null)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var memory in pendingAgent)
        {
            logger.LogInformation("Generating embedding for agent memory: {Title} ({AgentKey})", memory.Title, memory.AgentKey);
            var vector = await embeddingService.GenerateEmbeddingAsync(memory.Content, cancellationToken);
            if (vector.Length > 0)
            {
                memory.Embedding = vector;
            }
        }

        if (pendingGlobal.Count > 0 || pendingAgent.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Successfully synced {Count} embeddings.", pendingGlobal.Count + pendingAgent.Count);
        }
    }
}
