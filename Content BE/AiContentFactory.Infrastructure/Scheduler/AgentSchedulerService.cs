using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Scheduler;

public sealed class AgentSchedulerService(
    IServiceProvider serviceProvider,
    ILogger<AgentSchedulerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AgentSchedulerService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing AgentSchedulerService.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        logger.LogInformation("AgentSchedulerService is stopping.");
    }

    private async Task ProcessJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
        var now = DateTimeOffset.UtcNow;

        var dueJobs = await dbContext.ScheduleJobs
            .Where(j => j.IsEnabled && j.Status == "Active" && (j.NextRunAt == null || j.NextRunAt <= now))
            .ToListAsync(cancellationToken);

        foreach (var job in dueJobs)
        {
            logger.LogInformation("Processing schedule job {JobId}: {JobName}", job.Id, job.Name);
            
            job.Status = "Running";
            job.LastRunAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                // Simulate agent task execution
                await Task.Delay(1000, cancellationToken);
                
                job.Status = "Active";
                job.NextRunAt = CalculateNextRun(job.Trigger, now);
                await dbContext.SaveChangesAsync(cancellationToken);
                
                logger.LogInformation("Completed schedule job {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute job {JobId}", job.Id);
                job.Status = "Failed";
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
    
    private static DateTimeOffset? CalculateNextRun(string trigger, DateTimeOffset now)
    {
        if (trigger.Contains("Daily", StringComparison.OrdinalIgnoreCase))
        {
            return now.AddDays(1);
        }
        if (trigger.Contains("Hourly", StringComparison.OrdinalIgnoreCase))
        {
            return now.AddHours(1);
        }
        return now.AddMinutes(5); 
    }
}
