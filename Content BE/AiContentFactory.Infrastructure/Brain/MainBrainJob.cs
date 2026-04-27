using AiContentFactory.Application.Brain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Brain;

public sealed class MainBrainJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainBrainJob> _logger;

    public MainBrainJob(IServiceProvider serviceProvider, ILogger<MainBrainJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("MainBrainJob triggered by Hangfire.");
        using var scope = _serviceProvider.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IBrainOrchestrator>();
        
        await orchestrator.ExecuteTickAsync();
    }
}
