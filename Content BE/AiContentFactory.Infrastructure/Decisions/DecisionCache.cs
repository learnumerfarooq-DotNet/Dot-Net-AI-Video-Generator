using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Decisions;

public sealed class DecisionCache : IDecisionCache
{
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<DecisionCache> _logger;

    public DecisionCache(StudioDbContext dbContext, ILogger<DecisionCache> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var entry = await _dbContext.DecisionCacheEntries
            .FirstOrDefaultAsync(e => e.CacheKey == key, ct);

        if (entry == null) return null;

        if (entry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _dbContext.DecisionCacheEntries.Remove(entry);
            await _dbContext.SaveChangesAsync(ct);
            return null;
        }

        return entry.JsonPayload;
    }

    public async Task SetAsync(string key, string jsonPayload, TimeSpan ttl, CancellationToken ct = default)
    {
        var entry = await _dbContext.DecisionCacheEntries
            .FirstOrDefaultAsync(e => e.CacheKey == key, ct);

        if (entry == null)
        {
            entry = new DecisionCacheEntry
            {
                Id = Guid.NewGuid(),
                CacheKey = key,
                JsonPayload = jsonPayload,
                ExpiresAt = DateTimeOffset.UtcNow.Add(ttl)
            };
            _dbContext.DecisionCacheEntries.Add(entry);
        }
        else
        {
            entry.JsonPayload = jsonPayload;
            entry.ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
