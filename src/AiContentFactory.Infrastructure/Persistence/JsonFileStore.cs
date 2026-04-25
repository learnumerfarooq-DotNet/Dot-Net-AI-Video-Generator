using System.Text.Json;
using AiContentFactory.Application.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Persistence;

public sealed class JsonFileStore(
    IOptions<ContentFactoryOptions> options,
    IHostEnvironment environment) : IJsonFileStore
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<T> ReadAsync<T>(string fileName, T fallback, CancellationToken cancellationToken)
    {
        var path = GetPath(fileName);
        if (!File.Exists(path))
        {
            return fallback;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken) ?? fallback;
    }

    public async Task WriteAsync<T>(string fileName, T value, CancellationToken cancellationToken)
    {
        var path = GetPath(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, _jsonOptions, cancellationToken);
    }

    private string GetPath(string fileName)
    {
        var dataPath = options.Value.DataPath;
        var root = Path.IsPathRooted(dataPath)
            ? dataPath
            : Path.Combine(environment.ContentRootPath, dataPath);

        return Path.Combine(root, fileName);
    }
}
