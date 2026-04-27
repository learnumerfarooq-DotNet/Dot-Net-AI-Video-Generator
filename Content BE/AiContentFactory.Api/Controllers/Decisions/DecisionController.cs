using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Decisions;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Decisions;

[ApiController]
[Route("api/decisions")]
public class DecisionController : ControllerBase
{
    private readonly PromptVersionManager _versionManager;

    public DecisionController(PromptVersionManager versionManager)
    {
        _versionManager = versionManager;
    }

    [HttpGet("templates/{agentKey}/{type}")]
    public async Task<ActionResult<PromptTemplate>> GetActiveTemplate(string agentKey, DecisionType type, CancellationToken ct)
    {
        var template = await _versionManager.GetActiveTemplateAsync(agentKey, type, ct);
        return Ok(template);
    }

    [HttpGet("templates/{agentKey}/{type}/history")]
    public async Task<ActionResult<List<PromptTemplate>>> GetVersionHistory(string agentKey, DecisionType type, CancellationToken ct)
    {
        var history = await _versionManager.GetVersionHistoryAsync(agentKey, type, ct);
        return Ok(history);
    }

    [HttpPost("templates")]
    public async Task<ActionResult<PromptTemplate>> CreateNewVersion([FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var template = await _versionManager.CreateNewVersionAsync(
            request.AgentKey, request.Type, request.SystemPrompt, request.UserPromptTemplate, request.JsonOutputSchema, ct);
        return Ok(template);
    }

    [HttpPut("templates/{id}/activate")]
    public async Task<ActionResult> ActivateVersion(Guid id, CancellationToken ct)
    {
        await _versionManager.ActivateVersionAsync(id, ct);
        return NoContent();
    }
}

public class CreateTemplateRequest
{
    public string AgentKey { get; set; } = string.Empty;
    public DecisionType Type { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptTemplate { get; set; } = string.Empty;
    public string JsonOutputSchema { get; set; } = string.Empty;
}
