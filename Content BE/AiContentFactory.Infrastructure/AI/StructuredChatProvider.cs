using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiContentFactory.Application.AI;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.AI;

public sealed class StructuredChatOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string DefaultModel { get; set; } = "meta-llama/llama-4-maverick:free";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
}

public sealed class StructuredChatProvider(HttpClient httpClient, IOptions<StructuredChatOptions> options) : IStructuredChatProvider
{
    private readonly StructuredChatOptions _options = options.Value;

    public async Task<StructuredAIResponse> GetStructuredResponseAsync(string systemPrompt, string userPrompt, string jsonSchema, string? model = null, CancellationToken ct = default)
    {
        var url = $"{_options.BaseUrl}/chat/completions";
        
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        req.Headers.Add("X-Title", "AI Content Factory");
        
        var payload = new
        {
            model = model ?? _options.DefaultModel,
            messages = new[]
            {
                new { role = "system", content = $"{systemPrompt}\n\nIMPORTANT: You must return valid JSON that exactly matches this schema: {jsonSchema}" },
                new { role = "user", content = userPrompt }
            },
            response_format = new { type = "json_object" },
            temperature = 0.3
        };

        req.Content = JsonContent.Create(payload);

        var response = await httpClient.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(cancellationToken: ct);
        var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

        // Simple confidence score based on presence of content
        var confidence = string.IsNullOrWhiteSpace(content) ? 0.0 : 0.9;

        return new StructuredAIResponse(content, content, confidence);
    }

    private sealed class OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
