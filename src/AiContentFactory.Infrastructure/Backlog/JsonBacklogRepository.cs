using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Backlog;
using AiContentFactory.Infrastructure.Persistence;

namespace AiContentFactory.Infrastructure.Backlog;

public sealed class JsonBacklogRepository(IJsonFileStore store) : IBacklogRepository
{
    private const string BacklogFile = "backlog.items.json";
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<BacklogItem>> ListAsync(BacklogStatus? status, CancellationToken cancellationToken)
    {
        var items = await store.ReadAsync(BacklogFile, new List<BacklogItem>(), cancellationToken);
        return items
            .Where(item => status is null || item.Status == status)
            .OrderByDescending(item => item.CreatedAt)
            .ToArray();
    }

    public async Task<BacklogItem> AddAsync(BacklogItem item, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var items = await store.ReadAsync(BacklogFile, new List<BacklogItem>(), cancellationToken);
            items.Add(item);
            await store.WriteAsync(BacklogFile, items, cancellationToken);
            return item;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<BacklogItem?> UpdateStatusAsync(Guid id, BacklogStatus status, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var items = await store.ReadAsync(BacklogFile, new List<BacklogItem>(), cancellationToken);
            var index = items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return null;
            }

            items[index] = items[index] with { Status = status, UpdatedAt = DateTimeOffset.UtcNow };
            await store.WriteAsync(BacklogFile, items, cancellationToken);
            return items[index];
        }
        finally
        {
            _lock.Release();
        }
    }
}
