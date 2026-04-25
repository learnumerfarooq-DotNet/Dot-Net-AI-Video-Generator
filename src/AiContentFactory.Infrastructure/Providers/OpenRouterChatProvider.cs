using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AiContentFactory.Application.ContentFactory;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class OpenRouterChatProvider(HttpClient httpClient) : IChatProvider
{
    public string ProviderName => "OpenRouter";

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl) ? "https://openrouter.ai/api/v1/chat/completions" : baseUrl;
        
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        var payload = new
        {
            model = request.ModelName,
            messages = new List<object>
            {
                new { role = "system", content = request.SystemPrompt }
            }.Concat(request.History.Select(m => new { role = m.Role, content = m.Content }))
        };

        req.Content = JsonContent.Create(payload);

        var sw = Stopwatch.StartNew();
        var response = await httpClient.SendAsync(req, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(cancellationToken: cancellationToken);
        sw.Stop();

        var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response content generated.";
        var tokensIn = result?.Usage?.PromptTokens ?? 0;
        var tokensOut = result?.Usage?.CompletionTokens ?? 0;
        
        // Mock cost calculation for now, OpenRouter sends cost info via headers or separate API sometimes
        var cost = (decimal)(tokensIn + tokensOut) * 0.00001m; 

        return new ChatCompletionResult(
            content,
            tokensIn,
            tokensOut,
            cost,
            (int)sw.ElapsedMilliseconds);
    }

    private sealed class OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }
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

    private sealed class UsageInfo
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }
}
