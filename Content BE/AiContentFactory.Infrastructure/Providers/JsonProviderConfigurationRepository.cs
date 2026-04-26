using AiContentFactory.Application.Configuration;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class JsonProviderConfigurationRepository(
    IJsonFileStore store,
    IOptions<ContentFactoryOptions> options) : IProviderConfigurationRepository
{
    private const string ProvidersFile = "providers.config.json";
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<ProviderConfig> GetAsync(CancellationToken cancellationToken)
        => await store.ReadAsync(ProvidersFile, CreateDefault(), cancellationToken);

    public async Task<ProviderConfig> UpdateAsync(ProviderConfig config, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var clean = new ProviderConfig(
                Normalize(config.TextProvider, options.Value.TextProvider),
                Normalize(config.VideoProvider, options.Value.VideoProvider),
                Normalize(config.UploadProvider, options.Value.UploadProvider),
                Normalize(config.StorageProvider, options.Value.StorageProvider));

            await store.WriteAsync(ProvidersFile, clean, cancellationToken);
            return clean;
        }
        finally
        {
            _lock.Release();
        }
    }

    private ProviderConfig CreateDefault()
        => new(options.Value.TextProvider, options.Value.VideoProvider, options.Value.UploadProvider, options.Value.StorageProvider);

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
