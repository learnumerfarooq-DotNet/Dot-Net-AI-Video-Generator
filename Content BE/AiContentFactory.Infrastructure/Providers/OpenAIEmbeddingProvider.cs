using System.Net.Http.Json;
using System.Text.Json;
using AiContentFactory.Application.Studio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class OpenAIEmbeddingProvider(
    HttpClient httpClient,
    IServiceProvider serviceProvider,
    ILogger<OpenAIEmbeddingProvider> logger) : IEmbeddingService
{
    private const string EmbeddingModel = "text-embedding-3-small"; // 1536 dimensions

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

        // Get main-brain settings for API key
        using var scope = serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioWorkspaceStore>();
        var settings = await store.GetAgentSettingsAsync("main-brain", cancellationToken);
        if (settings == null || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured for 'main-brain'.");
        }

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var request = new
        {
            input = text,
            model = EmbeddingModel
        };

        var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken: cancellationToken);
        return result?.Data?.FirstOrDefault()?.Embedding ?? Array.Empty<float>();
    }

    private sealed class OpenAIEmbeddingResponse
    {
        public List<EmbeddingData>? Data { get; set; }
    }

    private sealed class EmbeddingData
    {
        public float[]? Embedding { get; set; }
    }
}
