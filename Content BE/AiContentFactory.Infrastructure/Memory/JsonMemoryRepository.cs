using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Infrastructure.Persistence;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class JsonMemoryRepository(IJsonFileStore store) : IMemoryRepository
{
    private const string EntriesFile = "memory.entries.json";
    private const string SuggestionsFile = "memory.suggestions.json";
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemorySearchRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var entries = await store.ReadAsync(EntriesFile, new List<MemoryEntry>(), cancellationToken);
        return entries
            .Where(entry => entry.ExpiresAt == null || entry.ExpiresAt > now)
            .Where(entry => request.Scope is null || entry.Scope == request.Scope)
            .Where(entry => string.IsNullOrWhiteSpace(request.AgentName) ||
                string.Equals(entry.AgentName, request.AgentName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.UpdatedAt)
            .Take(20)
            .ToArray();
    }

    public async Task<IReadOnlyList<MemorySuggestion>> GetPendingSuggestionsAsync(CancellationToken cancellationToken)
    {
        var suggestions = await store.ReadAsync(SuggestionsFile, new List<MemorySuggestion>(), cancellationToken);
        return suggestions
            .Where(suggestion => suggestion.Status == MemorySuggestionStatus.Pending)
            .OrderByDescending(suggestion => suggestion.CreatedAt)
            .ToArray();
    }

    public async Task<MemorySuggestion> SuggestAsync(MemorySuggestion suggestion, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var suggestions = await store.ReadAsync(SuggestionsFile, new List<MemorySuggestion>(), cancellationToken);
            suggestions.Add(suggestion);
            await store.WriteAsync(SuggestionsFile, suggestions, cancellationToken);
            return suggestion;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MemoryEntry?> ApproveSuggestionAsync(Guid suggestionId, string? revisedContent, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var suggestions = await store.ReadAsync(SuggestionsFile, new List<MemorySuggestion>(), cancellationToken);
            var index = suggestions.FindIndex(suggestion => suggestion.Id == suggestionId);
            if (index < 0)
            {
                return null;
            }

            var suggestion = suggestions[index];
            suggestions[index] = suggestion with { Status = MemorySuggestionStatus.Approved };

            var now = DateTimeOffset.UtcNow;
            var entry = new MemoryEntry(
                Guid.NewGuid(),
                suggestion.Scope,
                suggestion.AgentName,
                string.IsNullOrWhiteSpace(revisedContent) ? suggestion.Content : revisedContent,
                ["approved"],
                now,
                now,
                expiresAt);

            var entries = await store.ReadAsync(EntriesFile, new List<MemoryEntry>(), cancellationToken);
            entries.Add(entry);

            await store.WriteAsync(SuggestionsFile, suggestions, cancellationToken);
            await store.WriteAsync(EntriesFile, entries, cancellationToken);

            return entry;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RejectSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var suggestions = await store.ReadAsync(SuggestionsFile, new List<MemorySuggestion>(), cancellationToken);
            var index = suggestions.FindIndex(suggestion => suggestion.Id == suggestionId);
            if (index < 0)
            {
                return false;
            }

            suggestions[index] = suggestions[index] with { Status = MemorySuggestionStatus.Rejected };
            await store.WriteAsync(SuggestionsFile, suggestions, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MemoryEntry> SaveLocalAsync(string agentName, string content, IReadOnlyList<string> tags, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var entry = new MemoryEntry(Guid.NewGuid(), MemoryScope.Local, agentName, content, tags, now, now, expiresAt);
            var entries = await store.ReadAsync(EntriesFile, new List<MemoryEntry>(), cancellationToken);
            entries.Add(entry);
            await store.WriteAsync(EntriesFile, entries, cancellationToken);
            return entry;
        }
        finally
        {
            _lock.Release();
        }
    }
}
