using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Decisions;

public class PromptVersionManager
{
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<PromptVersionManager> _logger;

    public PromptVersionManager(StudioDbContext dbContext, ILogger<PromptVersionManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PromptTemplate> GetActiveTemplateAsync(string agentKey, DecisionType type, CancellationToken ct)
    {
        var template = await _dbContext.PromptTemplates
            .Where(t => t.AgentKey == agentKey && t.DecisionType == type && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (template == null)
        {
            _logger.LogWarning("No active template found for {AgentKey} / {DecisionType}. Falling back to latest.", agentKey, type);
            template = await _dbContext.PromptTemplates
                .Where(t => t.AgentKey == agentKey && t.DecisionType == type)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        return template ?? throw new Exception($"No template found for {agentKey} / {type}");
    }

    public async Task<PromptTemplate> CreateNewVersionAsync(string agentKey, DecisionType type, string system, string user, string schema, CancellationToken ct)
    {
        var currentVersion = await _dbContext.PromptTemplates
            .Where(t => t.AgentKey == agentKey && t.DecisionType == type)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.Version)
            .FirstOrDefaultAsync(ct) ?? "0.0";

        var newVersionNum = double.TryParse(currentVersion, out var v) ? v + 0.1 : 1.0;
        var newVersionString = newVersionNum.ToString("0.0");

        var newTemplate = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            AgentKey = agentKey,
            DecisionType = type,
            Version = newVersionString,
            SystemPrompt = system,
            UserPromptTemplate = user,
            JsonOutputSchema = schema,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.PromptTemplates.Add(newTemplate);
        await _dbContext.SaveChangesAsync(ct);

        return newTemplate;
    }

    public async Task ActivateVersionAsync(Guid templateId, CancellationToken ct)
    {
        var templateToActivate = await _dbContext.PromptTemplates.FindAsync(new object[] { templateId }, ct)
            ?? throw new Exception("Template not found");

        var currentActive = await _dbContext.PromptTemplates
            .Where(t => t.AgentKey == templateToActivate.AgentKey && t.DecisionType == templateToActivate.DecisionType && t.IsActive)
            .ToListAsync(ct);

        foreach (var t in currentActive)
        {
            t.IsActive = false;
        }

        templateToActivate.IsActive = true;
        templateToActivate.ActivatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<PromptTemplate>> GetVersionHistoryAsync(string agentKey, DecisionType type, CancellationToken ct)
    {
        return await _dbContext.PromptTemplates
            .Where(t => t.AgentKey == agentKey && t.DecisionType == type)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task RollbackAsync(string agentKey, DecisionType type, string version, CancellationToken ct)
    {
        var targetTemplate = await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.AgentKey == agentKey && t.DecisionType == type && t.Version == version, ct)
            ?? throw new Exception($"Template version {version} not found for {agentKey} / {type}");

        await ActivateVersionAsync(targetTemplate.Id, ct);
    }
}
