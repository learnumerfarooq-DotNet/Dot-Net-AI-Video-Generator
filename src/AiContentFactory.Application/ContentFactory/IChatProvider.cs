namespace AiContentFactory.Application.ContentFactory;

public sealed record ChatCompletionRequest(
    string SystemPrompt,
    IReadOnlyList<ChatMessageInput> History,
    string ModelName);

public sealed record ChatMessageInput(string Role, string Content);

public sealed record ChatCompletionResult(
    string Content,
    int TokensIn,
    int TokensOut,
    decimal CostUsd,
    int DurationMs);

public interface IChatProvider
{
    string ProviderName { get; }
    
    Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken);
}
