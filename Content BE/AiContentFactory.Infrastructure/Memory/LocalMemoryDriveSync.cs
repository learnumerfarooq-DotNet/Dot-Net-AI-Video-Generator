using AiContentFactory.Application.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class LocalMemoryDriveSync
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LocalMemoryDriveSync> _logger;

    public LocalMemoryDriveSync(IServiceProvider serviceProvider, ILogger<LocalMemoryDriveSync> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("LocalMemoryDriveSync triggered by Hangfire.");
        using var scope = _serviceProvider.CreateScope();
        var localMemoryService = scope.ServiceProvider.GetRequiredService<ILocalMemoryService>();
        
        var allMemories = await localMemoryService.GetAllAsync(ct);
        foreach (var memory in allMemories)
        {
            await localMemoryService.SyncToDriveAsync(memory.AgentKey, ct);
        }
    }
}
