using AiContentFactory.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class DrivePollingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DrivePollingBackgroundService> _logger;

    public DrivePollingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DrivePollingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Drive Polling Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var watcher = scope.ServiceProvider.GetRequiredService<IDriveFolderWatcher>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IPipelineOrchestrator>();

                var changes = await watcher.PollForChangesAsync("/RAW", stoppingToken);

                foreach (var change in changes)
                {
                    if (change.Type == ChangeType.Created)
                    {
                        await orchestrator.StartPipelineAsync(change.FileId, change.FileName, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Drive polling.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Drive Polling Service stopped.");
    }
}
