using AiContentFactory.Application.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class GlobalMemorySyncJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GlobalMemorySyncJob> _logger;

    public GlobalMemorySyncJob(IServiceProvider serviceProvider, ILogger<GlobalMemorySyncJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("GlobalMemorySyncJob triggered by Hangfire.");
        using var scope = _serviceProvider.CreateScope();
        var globalMemoryService = scope.ServiceProvider.GetRequiredService<IGlobalMemoryService>();
        
        await globalMemoryService.ForceRefreshAsync(ct);
    }
}
