using Microsoft.Extensions.DependencyInjection;
using AiContentFactory.Application.Studio;
using AiContentFactory.Application.ContentFactory;

namespace AiContentFactory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IStudioWorkspaceFacade, StudioWorkspaceFacade>();
        services.AddScoped<IContentFactoryFacade, ContentFactoryFacade>();
        return services;
    }
}
