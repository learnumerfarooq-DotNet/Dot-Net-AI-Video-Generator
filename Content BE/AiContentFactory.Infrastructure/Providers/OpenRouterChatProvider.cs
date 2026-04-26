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
        req.Headers.Add("X-Title", "AI Content Factory");
        req.Headers.Add("HTTP-Referer", "http://localhost:4200");
        
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

        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = System.Text.Json.JsonSerializer.Deserialize<OpenRouterResponse>(jsonString, options);
        sw.Stop();

        var content = result?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrEmpty(content))
        {
            content = "The model did not return a response. This may happen with free-tier models under heavy load — please try again.";
        }
        
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

    public async IAsyncEnumerable<string> StreamAsync(
        ChatCompletionRequest request,
        string apiKey,
        string baseUrl,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl) ? "https://openrouter.ai/api/v1/chat/completions" : baseUrl;
        
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Headers.Add("X-Title", "AI Content Factory");
        req.Headers.Add("HTTP-Referer", "http://localhost:4200");
        
        var payload = new
        {
            model = request.ModelName,
            stream = true,
            messages = new List<object>
            {
                new { role = "system", content = request.SystemPrompt }
            }.Concat(request.History.Select(m => new { role = m.Role, content = m.Content }))
        };

        req.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new System.IO.StreamReader(stream);

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("data: [DONE]")) break;
            if (line.StartsWith("data: "))
            {
                var json = line["data: ".Length..];
                OpenRouterStreamResponse? chunk = null;
                try {
                    chunk = System.Text.Json.JsonSerializer.Deserialize<OpenRouterStreamResponse>(json);
                } catch { /* ignore partial/malformed JSON in stream */ }

                var delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(delta))
                {
                    yield return delta;
                }
            }
        }
    }

    private sealed class OpenRouterStreamResponse
    {
        [JsonPropertyName("choices")]
        public List<StreamChoice>? Choices { get; set; }
    }

    private sealed class StreamChoice
    {
        [JsonPropertyName("delta")]
        public ChoiceDelta? Delta { get; set; }
    }

    private sealed class ChoiceDelta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
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
