namespace AiContentFactory.Infrastructure.Persistence;

public interface IJsonFileStore
{
    Task<T> ReadAsync<T>(string fileName, T fallback, CancellationToken cancellationToken);

    Task WriteAsync<T>(string fileName, T value, CancellationToken cancellationToken);
}
