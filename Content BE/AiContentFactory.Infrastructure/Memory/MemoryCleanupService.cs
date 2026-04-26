using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class MemoryCleanupService(
    IServiceProvider serviceProvider,
    ILogger<MemoryCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Memory Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredMemoriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while cleaning up expired memories.");
            }

            // Run once every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CleanupExpiredMemoriesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
        var now = DateTimeOffset.UtcNow;

        logger.LogInformation("Checking for expired memories...");

        var expiredGlobalCount = await dbContext.GlobalMemories
            .Where(m => m.ExpiresAt != null && m.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        var expiredAgentCount = await dbContext.AgentMemories
            .Where(m => m.ExpiresAt != null && m.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        if (expiredGlobalCount > 0 || expiredAgentCount > 0)
        {
            logger.LogInformation("Cleanup complete. Removed {GlobalCount} global and {AgentCount} agent memories.", expiredGlobalCount, expiredAgentCount);
        }
    }
}
