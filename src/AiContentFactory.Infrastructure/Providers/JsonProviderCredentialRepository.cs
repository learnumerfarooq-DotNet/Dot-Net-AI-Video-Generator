using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Infrastructure.Persistence;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class JsonProviderCredentialRepository(IJsonFileStore store) : IProviderCredentialRepository
{
    private const string CredentialsFile = "providers.credentials.json";
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task SaveManyAsync(IReadOnlyList<ProviderCredentialInput> credentials, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var records = await store.ReadAsync(CredentialsFile, new List<ProviderCredentialRecord>(), cancellationToken);

            foreach (var credential in credentials)
            {
                var cleanValues = credential.Values
                    .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                    .ToDictionary(pair => pair.Key, pair => pair.Value.Trim());

                if (cleanValues.Count == 0)
                {
                    continue;
                }

                var record = new ProviderCredentialRecord(
                    credential.ProviderType,
                    credential.ProviderName,
                    cleanValues,
                    DateTimeOffset.UtcNow);

                var index = records.FindIndex(item =>
                    string.Equals(item.ProviderType, credential.ProviderType, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.ProviderName, credential.ProviderName, StringComparison.OrdinalIgnoreCase));

                if (index >= 0)
                {
                    records[index] = record;
                }
                else
                {
                    records.Add(record);
                }
            }

            await store.WriteAsync(CredentialsFile, records, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<ProviderCredentialStatus>> GetStatusAsync(CancellationToken cancellationToken)
    {
        var records = await store.ReadAsync(CredentialsFile, new List<ProviderCredentialRecord>(), cancellationToken);
        return records
            .Select(record => new ProviderCredentialStatus(
                record.ProviderType,
                record.ProviderName,
                record.Values.ToDictionary(pair => pair.Key, pair => !string.IsNullOrWhiteSpace(pair.Value)),
                record.UpdatedAt))
            .ToArray();
    }

    private sealed record ProviderCredentialRecord(
        string ProviderType,
        string ProviderName,
        IReadOnlyDictionary<string, string> Values,
        DateTimeOffset UpdatedAt);
}
