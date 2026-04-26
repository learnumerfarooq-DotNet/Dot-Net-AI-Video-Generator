namespace AiContentFactory.Application.Studio;

public interface IEmbeddingService
{
    /// <summary>
    /// Generates a vector embedding for the given text.
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
}
