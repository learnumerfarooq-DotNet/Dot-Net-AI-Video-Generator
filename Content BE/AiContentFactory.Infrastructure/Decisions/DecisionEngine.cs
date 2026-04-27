using System.Diagnostics;
using System.Text.Json;
using AiContentFactory.Application.AI;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Decisions;

public sealed class DecisionEngine : IDecisionEngine
{
    private readonly IStructuredChatProvider _chatProvider;
    private readonly IDecisionValidator _validator;
    private readonly StudioDbContext _dbContext;
    private readonly IDecisionCache _cache;
    private readonly ILogger<DecisionEngine> _logger;

    public DecisionEngine(
        IStructuredChatProvider chatProvider,
        IDecisionValidator validator,
        StudioDbContext dbContext,
        IDecisionCache cache,
        ILogger<DecisionEngine> logger)
    {
        _chatProvider = chatProvider;
        _validator = validator;
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    private (string Primary, string Fallback) GetModelsForAgent(string agentKey)
    {
        return agentKey switch
        {
            "script-gen-agent" => ("llama-3.1-8b-instruct:free", "mistral-7b:free"),
            "edit-agent" => ("gemini-flash-1.5-8b:free", "llama-3.1-8b:free"),
            "shorts-agent" => ("llama-3.2-90b:free", "llama-3.1-8b:free"),
            "short-edit-agent" => ("gemini-flash-1.5-8b:free", "llama-3.1-8b:free"),
            "trend-agent" => ("mistral-7b:free", "llama-3.1-8b:free"),
            "upload-agent" => ("llama-3.1-8b:free", "mistral-7b:free"),
            "analytics-agent" => ("llama-3.1-8b:free", "mistral-7b:free"),
            "main-brain" => ("llama-4-maverick:free", "llama-3.1-8b:free"),
            _ => ("llama-3.1-8b-instruct:free", "llama-3.1-8b:free")
        };
    }

    public async Task<AgentDecision> MakeDecisionAsync(string agentKey, DecisionType type, Dictionary<string, string> context, Guid? jobId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Making decision of type {Type} for agent {AgentKey}", type, agentKey);

        var template = await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.AgentKey == agentKey && t.DecisionType == type && t.IsActive, ct)
            ?? throw new InvalidOperationException($"No active prompt template found for agent {agentKey} and decision {type}");

        var contextJson = JsonSerializer.Serialize(context);
        var cacheKey = $"decision:{agentKey}:{type}:{contextJson.GetHashCode()}";

        var cachedResponse = await _cache.GetAsync(cacheKey, ct);
        if (cachedResponse != null)
        {
            var cachedDecision = new AgentDecision
            {
                Id = Guid.NewGuid(),
                AgentKey = agentKey,
                Type = type,
                Outcome = DecisionOutcome.Validated,
                RawJsonPayload = cachedResponse,
                ValidatedPayload = cachedResponse,
                ConfidenceScore = 1.0,
                PromptVersion = template.Version,
                JobId = jobId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.AgentDecisions.Add(cachedDecision);
            await _dbContext.SaveChangesAsync(ct);
            return cachedDecision;
        }

        var userPrompt = template.UserPromptTemplate;
        foreach (var (key, value) in context)
        {
            userPrompt = userPrompt.Replace($"{{{key}}}", value);
        }

        var models = GetModelsForAgent(agentKey);
        string[] modelsToTry = { models.Primary, models.Fallback, "llama-3.1-8b:free" };
        
        StructuredAIResponse? lastResponse = null;
        AgentDecision? decision = null;
        long totalLatencyMs = 0;

        for (int attempt = 0; attempt < 3; attempt++) // Max 2 retries (3 attempts total)
        {
            var modelToUse = modelsToTry[Math.Min(attempt, modelsToTry.Length - 1)];
            
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Attempt {Attempt}: Calling AI with model {Model}", attempt + 1, modelToUse);
                lastResponse = await _chatProvider.GetStructuredResponseAsync(template.SystemPrompt, userPrompt, template.JsonOutputSchema, modelToUse, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI Call failed on attempt {Attempt} with model {Model}", attempt + 1, modelToUse);
                continue; // Try next model/retry
            }
            finally
            {
                sw.Stop();
                totalLatencyMs += sw.ElapsedMilliseconds;
            }

            if (lastResponse == null) continue;

            decision = new AgentDecision
            {
                Id = Guid.NewGuid(),
                AgentKey = agentKey,
                Type = type,
                Outcome = DecisionOutcome.Pending,
                RawJsonPayload = lastResponse.JsonPayload,
                ConfidenceScore = lastResponse.ConfidenceScore,
                PromptVersion = template.Version,
                JobId = jobId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var validation = await _validator.ValidateAsync(decision, ct);
            if (validation.IsValid)
            {
                decision.Outcome = DecisionOutcome.Validated;
                decision.ValidatedPayload = decision.RawJsonPayload;
                await _cache.SetAsync(cacheKey, decision.ValidatedPayload, TimeSpan.FromHours(1), ct);
                break; // Success!
            }
            else
            {
                decision.Outcome = DecisionOutcome.Failed;
                _logger.LogWarning("Decision validation failed on attempt {Attempt}: {Error}", attempt + 1, validation.ErrorMessage);
            }
        }

        if (decision == null)
        {
            throw new Exception("All decision attempts and model fallbacks failed.");
        }

        var auditLog = new DecisionAuditLog
        {
            Id = Guid.NewGuid(),
            DecisionId = decision.Id,
            AgentKey = agentKey,
            DecisionType = type,
            InputContextHash = contextJson.GetHashCode().ToString(),
            RawResponse = lastResponse?.RawResponse ?? "",
            ValidatedResponse = decision.ValidatedPayload,
            ConfidenceScore = decision.ConfidenceScore,
            LatencyMs = totalLatencyMs,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AgentDecisions.Add(decision);
        _dbContext.DecisionAuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(ct);

        return decision;
    }

    public Task<T?> ParsePayloadAsync<T>(AgentDecision decision) where T : class
    {
        try
        {
            var payload = JsonSerializer.Deserialize<T>(decision.ValidatedPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Task.FromResult(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse decision payload");
            return Task.FromResult<T?>(null);
        }
    }
}
