using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/script-gen")]
public class ScriptGenController : ControllerBase
{
    private readonly IScriptGenAgent _scriptGenAgent;

    public ScriptGenController(IScriptGenAgent scriptGenAgent)
    {
        _scriptGenAgent = scriptGenAgent;
    }

    [HttpPost("generate/{jobId}")]
    public async Task<ActionResult<ScriptOutput>> GenerateScript(Guid jobId, CancellationToken ct)
    {
        // Ideally we fetch the metadata for this job, but for the API demo we'll just mock it.
        // The real pipeline handles this in RawPipelineHandler.
        var metadata = new AiContentFactory.Domain.Pipeline.VideoMetadata {  };
        var script = await _scriptGenAgent.GenerateScriptAsync(jobId, metadata, ct);
        return Ok(script);
    }

    [HttpPost("regenerate/{jobId}")]
    public async Task<ActionResult<ScriptOutput>> RegenerateScript(Guid jobId, [FromQuery] string style, [FromQuery] string tone, CancellationToken ct)
    {
        var script = await _scriptGenAgent.RegenerateScriptAsync(jobId, style, tone, ct);
        return Ok(script);
    }

    [HttpGet("script/{jobId}")]
    public async Task<ActionResult<ScriptOutput>> GetScript(Guid jobId, CancellationToken ct)
    {
        var script = await _scriptGenAgent.GetScriptAsync(jobId, ct);
        if (script == null) return NotFound();
        return Ok(script);
    }

    [HttpPost("validate/{jobId}")]
    public async Task<ActionResult<bool>> ValidateScript(Guid jobId, CancellationToken ct)
    {
        var script = await _scriptGenAgent.GetScriptAsync(jobId, ct);
        if (script == null) return NotFound();
        
        var isValid = await _scriptGenAgent.ValidateScriptAsync(script, ct);
        return Ok(isValid);
    }
}
