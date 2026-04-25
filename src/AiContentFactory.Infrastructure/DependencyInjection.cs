using AiContentFactory.Application.Configuration;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Agents;
using AiContentFactory.Infrastructure.Backlog;
using AiContentFactory.Infrastructure.Memory;
using AiContentFactory.Infrastructure.Persistence;
using AiContentFactory.Infrastructure.Providers;
using AiContentFactory.Infrastructure.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiContentFactory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ContentFactoryOptions>(configuration.GetSection(ContentFactoryOptions.SectionName));
        services.AddDbContext<StudioDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
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
        services.AddHostedService<AgentSchedulerService>();
        return services;
    }
}
