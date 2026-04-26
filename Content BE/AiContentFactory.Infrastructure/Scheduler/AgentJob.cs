using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AiContentFactory.Infrastructure.Scheduler;

/// <summary>
/// A Quartz job that executes an agent's scheduled task.
/// </summary>
[DisallowConcurrentExecution]
public sealed class AgentJob(
    IServiceScopeFactory scopeFactory,
    IMaskingService masking,
    ILogger<AgentJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jobGuidStr = context.MergedJobDataMap.GetString("JobId");
        if (!Guid.TryParse(jobGuidStr, out var jobId))
        {
            logger.LogWarning("AgentJob triggered without a valid JobId.");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<IWorkspaceNotificationService>();
        
        var job = await db.ScheduleJobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null || !job.IsEnabled)
        {
            logger.LogInformation("Job {JobId} is either missing or disabled. Skipping.", jobId);
            return;
        }

        logger.LogInformation("Quartz executing job {JobName} for agent {AgentKey}", job.Name, job.AgentKey);

        job.Status = "Running";
        job.LastRunAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var runIdStr = context.MergedJobDataMap.GetString("RunId");
        Guid? existingRunId = Guid.TryParse(runIdStr, out var rid) ? rid : null;

        var run = existingRunId.HasValue 
            ? await db.AgentRuns.FirstOrDefaultAsync(r => r.Id == existingRunId.Value)
            : null;

        if (run == null)
        {
            run = new StudioAgentRunEntity
            {
                Id = Guid.NewGuid(),
                AgentKey = job.AgentKey ?? "system",
                Title = $"Scheduled: {job.Name}",
                Status = "Running",
                QueuedAt = DateTimeOffset.UtcNow,
                MaxRetries = 3,
                AttemptCount = 1,
                ExecutionLog = $"[{DateTimeOffset.UtcNow:u}] Job '{job.Name}' started by Quartz scheduler.\n"
            };
            db.AgentRuns.Add(run);
        }
        else
        {
            run.Status = "Running";
            run.AttemptCount++;
            run.ExecutionLog += $"[{DateTimeOffset.UtcNow:u}] Retry attempt {run.AttemptCount} starting.\n";
        }

        await db.SaveChangesAsync();
        await notifications.NotifyAgentRunStartedAsync(run.Id, run.AgentKey, context.CancellationToken);

        try
        {
            // Simulate work
            await Task.Delay(3000, context.CancellationToken); 

            run.Status = "Completed";
            run.Summary = $"Successfully processed {job.Name} cycle.";
            run.CompletedAt = DateTimeOffset.UtcNow;
            run.ExecutionLog += $"[{DateTimeOffset.UtcNow:u}] Pipeline completed successfully.";

            job.Status = "Active";
            job.Notes = $"Last successful run: {DateTimeOffset.UtcNow:u}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running job {JobId}, attempt {Attempt}", jobId, run.AttemptCount);
            
            run.Summary = $"Execution failed: {ex.Message}";
            run.ExecutionLog += $"[{DateTimeOffset.UtcNow:u}] Error: {ex.Message}\n";

            if (run.AttemptCount < run.MaxRetries)
            {
                run.Status = "Retrying";
                var backoffSeconds = (int)Math.Pow(2, run.AttemptCount) * 30; // 60s, 120s, 240s...
                
                run.ExecutionLog += $"[{DateTimeOffset.UtcNow:u}] Scheduling retry in {backoffSeconds} seconds.\n";
                
                var retryTrigger = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.UtcNow.AddSeconds(backoffSeconds))
                    .UsingJobData("JobId", jobId.ToString())
                    .UsingJobData("RunId", run.Id.ToString())
                    .Build();

                await context.Scheduler.ScheduleJob(context.JobDetail, new[] { retryTrigger }, true);
                job.Status = "Retrying";
            }
            else
            {
                run.Status = "Failed";
                run.CompletedAt = DateTimeOffset.UtcNow;
                run.ExecutionLog += $"[{DateTimeOffset.UtcNow:u}] Maximum retries reached. Execution aborted.";
                job.Status = "Failed";
            }
        }

        run.ExecutionLog = masking.Scrub(run.ExecutionLog);
        await db.SaveChangesAsync();
        await notifications.NotifyAgentRunCompletedAsync(run.Id, run.AgentKey, run.Status, context.CancellationToken);
    }
}
