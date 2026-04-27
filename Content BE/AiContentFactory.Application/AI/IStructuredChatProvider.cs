namespace AiContentFactory.Application.AI;

public interface IStructuredChatProvider
{
    Task<StructuredAIResponse> GetStructuredResponseAsync(string systemPrompt, string userPrompt, string jsonSchema, string? model = null, CancellationToken ct = default);
}

public record StructuredAIResponse(string RawResponse, string JsonPayload, double ConfidenceScore);

public interface IAIService
{
    Task<string> GetResponseAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
