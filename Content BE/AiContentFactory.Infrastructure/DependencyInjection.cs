using AiContentFactory.Application.Configuration;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Agents;
using AiContentFactory.Infrastructure.Backlog;
using AiContentFactory.Infrastructure.Memory;
using AiContentFactory.Infrastructure.Persistence;
using AiContentFactory.Infrastructure.Providers;
using AiContentFactory.Infrastructure.Scheduler;
using AiContentFactory.Infrastructure.Security;
using AiContentFactory.Infrastructure.Storage;
using AiContentFactory.Infrastructure.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace AiContentFactory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ContentFactoryOptions>(configuration.GetSection(ContentFactoryOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        
        services.AddDbContext<StudioDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
            
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IMaskingService, MaskingService>();
        services.AddHostedService<DriveSyncService>();
        services.AddHostedService<EmbeddingSyncService>();
        services.AddHostedService<MemoryCleanupService>();
        services.AddScoped<IStudioWorkspaceStore, StudioWorkspaceStore>();
        services.AddSingleton<IJsonFileStore, JsonFileStore>();
        services.AddScoped<IMemoryRepository, DbMemoryRepository>();
        services.AddScoped<IBacklogRepository, DbBacklogRepository>();
        services.AddSingleton<IProviderConfigurationRepository, JsonProviderConfigurationRepository>();
        services.AddSingleton<IProviderCredentialRepository, JsonProviderCredentialRepository>();
        services.AddSingleton<IProviderRequirementCatalog, ProviderRequirementCatalog>();
        services.AddSingleton<ITextGenerationProvider, TemplateTextGenerationProvider>();
        services.AddSingleton<IUploadExecutionProvider, DryRunUploadExecutionProvider>();
        services.AddHttpClient<IChatProvider, OpenRouterChatProvider>();
        services.AddScoped<IContentAgent, ScriptAgent>();
        services.AddScoped<IContentAgent, UploadAgent>();
        services.AddHttpClient<IGoogleDriveService, GoogleDriveService>();
        services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingProvider>();
        
        // Agent Tools
        services.AddScoped<IStudioTool, TrendSearchTool>();
        services.AddScoped<IStudioTool, ScriptDraftTool>();
        services.AddScoped<IStudioTool, ScheduleTaskTool>();
        services.AddScoped<StudioToolRegistry>();
        
        return services;
    }
}
