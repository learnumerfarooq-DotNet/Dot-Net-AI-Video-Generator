using AiContentFactory.Application.Studio;
using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class DbMemoryRepository(
    StudioDbContext dbContext,
    IEmbeddingService embeddingService) : IMemoryRepository
{
    public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemorySearchRequest request, CancellationToken cancellationToken)
    {
        // Simple keyword/recent search since pgvector is not available
        var now = DateTimeOffset.UtcNow;
        var globalQuery = dbContext.GlobalMemories
            .Where(m => m.Status == "Approved")
            .Where(m => m.ExpiresAt == null || m.ExpiresAt > now)
            .Select(m => new { m.Id, Scope = "Global", AgentKey = (string?)null, m.Content, m.Tags, m.CreatedAt, m.UpdatedAt, m.ExpiresAt });

        var agentQuery = dbContext.AgentMemories
            .Where(m => m.Status == "Approved")
            .Where(m => m.ExpiresAt == null || m.ExpiresAt > now)
            .Select(m => new { m.Id, Scope = "Local", AgentKey = (string?)m.AgentKey, m.Content, m.Tags, m.CreatedAt, m.UpdatedAt, m.ExpiresAt });

        var combined = globalQuery.Concat(agentQuery);

        if (request.Scope is not null)
        {
            var scopeStr = request.Scope.ToString();
            combined = combined.Where(m => m.Scope == scopeStr);
        }

        if (!string.IsNullOrWhiteSpace(request.AgentName))
        {
            combined = combined.Where(m => m.AgentKey == request.AgentName);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            // Simple string containment for keyword search
            combined = combined.Where(m => m.Content.Contains(request.Query));
        }

        var rows = await combined
            .OrderByDescending(m => m.UpdatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        return rows.Select(m => new MemoryEntry(
            m.Id,
            Enum.Parse<MemoryScope>(m.Scope, true),
            m.AgentKey,
            m.Content)
        {
            Tags = m.Tags.ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            ExpiresAt = m.ExpiresAt,
            Score = 1.0f
        }).ToArray();
    }

    public async Task<IReadOnlyList<MemorySuggestion>> GetPendingSuggestionsAsync(CancellationToken cancellationToken)
    {
        var globalRows = await dbContext.GlobalMemories
            .Where(m => m.Status == "Pending")
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var agentRows = await dbContext.AgentMemories
            .Where(m => m.Status == "Pending")
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var globalSuggestions = globalRows.Select(m => new MemorySuggestion(m.Id, MemoryScope.Global, null, m.Content, "Pending review") { Status = MemorySuggestionStatus.Pending, CreatedAt = m.CreatedAt });
        var agentSuggestions = agentRows.Select(m => new MemorySuggestion(m.Id, MemoryScope.Local, m.AgentKey, m.Content, "Pending review") { Status = MemorySuggestionStatus.Pending, CreatedAt = m.CreatedAt });

        return globalSuggestions.Concat(agentSuggestions).OrderByDescending(s => s.CreatedAt).ToArray();
    }

    public async Task<MemorySuggestion> SuggestAsync(MemorySuggestion suggestion, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (suggestion.Scope == MemoryScope.Global)
        {
            dbContext.GlobalMemories.Add(new StudioGlobalMemoryEntity
            {
                Id = suggestion.Id,
                Title = $"Suggested: {suggestion.Content[..Math.Min(80, suggestion.Content.Length)]}",
                Content = suggestion.Content,
                Status = "Pending",
                Tags = [],
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            dbContext.AgentMemories.Add(new StudioAgentMemoryEntity
            {
                Id = suggestion.Id,
                AgentKey = suggestion.AgentName ?? string.Empty,
                Title = $"Suggested: {suggestion.Content[..Math.Min(80, suggestion.Content.Length)]}",
                Content = suggestion.Content,
                Status = "Pending",
                Tags = [],
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return suggestion;
    }

    public async Task<MemoryEntry?> ApproveSuggestionAsync(Guid suggestionId, string? revisedContent, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var global = await dbContext.GlobalMemories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (global != null)
        {
            if (!string.IsNullOrWhiteSpace(revisedContent)) global.Content = revisedContent;
            global.Status = "Approved"; global.UpdatedAt = now; global.ApprovedAt = now;
            global.ExpiresAt = expiresAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new MemoryEntry(global.Id, MemoryScope.Global, null, global.Content)
            {
                Tags = global.Tags.ToList(),
                CreatedAt = global.CreatedAt,
                UpdatedAt = global.UpdatedAt,
                ExpiresAt = global.ExpiresAt
            };
        }

        var agent = await dbContext.AgentMemories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (agent != null)
        {
            if (!string.IsNullOrWhiteSpace(revisedContent)) agent.Content = revisedContent;
            agent.Status = "Approved"; agent.UpdatedAt = now; agent.ApprovedAt = now;
            agent.ExpiresAt = expiresAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new MemoryEntry(agent.Id, MemoryScope.Local, agent.AgentKey, agent.Content)
            {
                Tags = agent.Tags.ToList(),
                CreatedAt = agent.CreatedAt,
                UpdatedAt = agent.UpdatedAt,
                ExpiresAt = agent.ExpiresAt
            };
        }

        return null;
    }

    public async Task<bool> RejectSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken)
    {
        var global = await dbContext.GlobalMemories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (global != null) { global.Status = "Rejected"; global.UpdatedAt = DateTimeOffset.UtcNow; await dbContext.SaveChangesAsync(cancellationToken); return true; }

        var agent = await dbContext.AgentMemories.FirstOrDefaultAsync(m => m.Id == suggestionId, cancellationToken);
        if (agent != null) { agent.Status = "Rejected"; agent.UpdatedAt = DateTimeOffset.UtcNow; await dbContext.SaveChangesAsync(cancellationToken); return true; }

        return false;
    }

    public async Task<MemoryEntry> SaveLocalAsync(string agentName, string content, IReadOnlyList<string> tags, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid();

        float[]? embedding = null;
        try
        {
            var vector = await embeddingService.GenerateEmbeddingAsync(content, cancellationToken);
            if (vector.Length > 0) embedding = vector;
        }
        catch { /* fallback to background sync if service is down */ }

        dbContext.AgentMemories.Add(new StudioAgentMemoryEntity
        {
            Id = id,
            AgentKey = agentName,
            Title = $"Local: {content[..Math.Min(80, content.Length)]}",
            Content = content,
            Status = "Approved",
            Tags = tags.ToArray(),
            CreatedAt = now,
            UpdatedAt = now,
            ApprovedAt = now,
            ExpiresAt = expiresAt,
            Embedding = embedding
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return new MemoryEntry(id, MemoryScope.Local, agentName, content)
        {
            Tags = tags.ToList(),
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = expiresAt,
            Score = 1.0f
        };
    }
}
