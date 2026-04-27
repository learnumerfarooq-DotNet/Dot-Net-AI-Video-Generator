using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/short-edit")]
public class ShortEditController : ControllerBase
{
    private readonly IShortEditAgent _shortEditAgent;

    public ShortEditController(IShortEditAgent shortEditAgent)
    {
        _shortEditAgent = shortEditAgent;
    }

    [HttpPost("plan/{shortClipId}")]
    public async Task<ActionResult<ShortEditPlan>> CreateEditPlan(Guid shortClipId, CancellationToken ct)
    {
        var plan = await _shortEditAgent.CreateEditPlanAsync(shortClipId, ct);
        return Ok(plan);
    }

    [HttpPost("execute/{jobId}")]
    public async Task<IActionResult> ExecuteEditPlan(Guid jobId, [FromBody] ShortEditPlan plan, CancellationToken ct)
    {
        await _shortEditAgent.ExecuteEditPlanAsync(jobId, plan, ct);
        return Ok();
    }

    [HttpGet("plan/{shortClipId}")]
    public async Task<ActionResult<ShortEditPlan>> GetEditPlan(Guid shortClipId, CancellationToken ct)
    {
        var plan = await _shortEditAgent.GetEditPlanAsync(shortClipId, ct);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpPost("re-execute/{shortClipId}")]
    public async Task<IActionResult> ReExecute(Guid shortClipId, CancellationToken ct)
    {
        await _shortEditAgent.ReExecuteAsync(shortClipId, ct);
        return Ok();
    }
}
