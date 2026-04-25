using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Memory;

/// <summary>
/// PostgreSQL-backed implementation of IMemoryRepository.
/// Replaces the JSON file store with direct EF Core queries against studio_memories.
/// </summary>
public sealed class DbMemoryRepository(StudioDbContext dbContext) : IMemoryRepository
{
    public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemorySearchRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Memories
            .Where(m => m.Status == "Approved")
            .AsQueryable();

        if (request.Scope is not null)
        {
            var scopeStr = request.Scope.ToString();
            query = query.Where(m => m.Scope == scopeStr);
        }

        if (!string.IsNullOrWhiteSpace(request.AgentName))
        {
            query = query.Where(m => m.AgentKey == request.AgentName);
        }

        var rows = await query
            .OrderByDescending(m => m.UpdatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        return rows.Select(m => new MemoryEntry(
            m.Id,
            Enum.Parse<MemoryScope>(m.Scope, true),
            m.AgentKey,
            m.Content,
            m.Tags,
            m.CreatedAt,
            m.UpdatedAt)).ToArray();
    }

    public async Task<IReadOnlyList<MemorySuggestion>> GetPendingSuggestionsAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.Memories
            .Where(m => m.Status == "Pending")
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(m => new MemorySuggestion(
            m.Id,
            Enum.Parse<MemoryScope>(m.Scope, true),
            m.AgentKey,
            m.Content,
            "Pending review from agent",
            MemorySuggestionStatus.Pending,
            m.CreatedAt)).ToArray();
    }

    public async Task<MemorySuggestion> SuggestAsync(MemorySuggestion suggestion, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        dbContext.Memories.Add(new StudioMemoryEntity
        {
            Id = suggestion.Id,
            Scope = suggestion.Scope.ToString(),
            AgentKey = suggestion.AgentName,
            Title = $"Suggested: {suggestion.Content[..Math.Min(80, suggestion.Content.Length)]}",
            Content = suggestion.Content,
            Status = "Pending",
            Tags = [],
            CreatedAt = now,
            UpdatedAt = now,
            ApprovedAt = null
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return suggestion;
    }

    public async Task<MemoryEntry?> ApproveSuggestionAsync(Guid suggestionId, string? revisedContent, CancellationToken cancellationToken)
    {
        var memory = await dbContext.Memories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (memory is null) return null;

        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(revisedContent))
            memory.Content = revisedContent;

        memory.Status = "Approved";
        memory.UpdatedAt = now;
        memory.ApprovedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new MemoryEntry(
            memory.Id,
            Enum.Parse<MemoryScope>(memory.Scope, true),
            memory.AgentKey,
            memory.Content,
            memory.Tags,
            memory.CreatedAt,
            memory.UpdatedAt);
    }

    public async Task<bool> RejectSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken)
    {
        var memory = await dbContext.Memories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (memory is null) return false;

        memory.Status = "Rejected";
        memory.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MemoryEntry> SaveLocalAsync(string agentName, string content, IReadOnlyList<string> tags, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid();

        dbContext.Memories.Add(new StudioMemoryEntity
        {
            Id = id,
            Scope = "Local",
            AgentKey = agentName,
            Title = $"Local: {content[..Math.Min(80, content.Length)]}",
            Content = content,
            Status = "Approved",
            Tags = tags.ToArray(),
            CreatedAt = now,
            UpdatedAt = now,
            ApprovedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new MemoryEntry(id, MemoryScope.Local, agentName, content, tags, now, now);
    }
}
