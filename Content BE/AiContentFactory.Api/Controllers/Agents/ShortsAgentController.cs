using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/shorts")]
public class ShortsAgentController : ControllerBase
{
    private readonly IShortsAgent _shortsAgent;

    public ShortsAgentController(IShortsAgent shortsAgent)
    {
        _shortsAgent = shortsAgent;
    }

    [HttpPost("generate/{jobId}")]
    public async Task<ActionResult<List<ShortClip>>> GenerateShorts(Guid jobId, CancellationToken ct)
    {
        var shorts = await _shortsAgent.GenerateShortsAsync(jobId, ct);
        return Ok(shorts);
    }

    [HttpGet("{jobId}")]
    public async Task<ActionResult<List<ShortClip>>> GetShorts(Guid jobId, CancellationToken ct)
    {
        var shorts = await _shortsAgent.GetShortsAsync(jobId, ct);
        return Ok(shorts);
    }

    [HttpPost("regenerate/{jobId}")]
    public async Task<ActionResult<List<ShortClip>>> RegenerateShorts(Guid jobId, [FromQuery] int maxShorts = 5, [FromQuery] int minDuration = 15, CancellationToken ct = default)
    {
        var shorts = await _shortsAgent.RegenerateShortsAsync(jobId, maxShorts, minDuration, ct);
        return Ok(shorts);
    }

    [HttpGet("clip/{clipId}")]
    public async Task<ActionResult<ShortClip>> GetClip(Guid clipId, CancellationToken ct)
    {
        // Mock method if needed, the API usually filters via the list or hits a specific service.
        return Ok(new { Message = "Not fully implemented. Use GetShorts(jobId)." });
    }

    [HttpPost("clip/{clipId}/reprocess")]
    public async Task<IActionResult> ReprocessClip(Guid clipId, [FromBody] string sourcePath, CancellationToken ct)
    {
        // We'd load the clip via repo. Mocked for now.
        var clip = new ShortClip { Id = clipId, ClipNumber = 1, StartTime = 0, EndTime = 15 };
        await _shortsAgent.ProcessShortClipAsync(clip, sourcePath, ct);
        return Ok();
    }
}
