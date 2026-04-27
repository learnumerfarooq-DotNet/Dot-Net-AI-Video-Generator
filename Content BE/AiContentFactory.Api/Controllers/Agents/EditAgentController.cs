using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/edit")]
public class EditAgentController : ControllerBase
{
    private readonly IEditAgent _editAgent;

    public EditAgentController(IEditAgent editAgent)
    {
        _editAgent = editAgent;
    }

    [HttpPost("plan/{jobId}")]
    public async Task<ActionResult<EditPlan>> CreateEditPlan(Guid jobId, CancellationToken ct)
    {
        var plan = await _editAgent.CreateEditPlanAsync(jobId, ct);
        return Ok(plan);
    }

    [HttpPost("execute/{jobId}")]
    public async Task<IActionResult> ExecuteEditPlan(Guid jobId, [FromBody] EditPlan plan, CancellationToken ct)
    {
        await _editAgent.ExecuteEditPlanAsync(jobId, plan, ct);
        return Ok();
    }

    [HttpGet("plan/{jobId}")]
    public async Task<ActionResult<EditPlan>> GetEditPlan(Guid jobId, CancellationToken ct)
    {
        var plan = await _editAgent.GetEditPlanAsync(jobId, ct);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpPost("re-execute/{jobId}")]
    public async Task<IActionResult> ReExecute(Guid jobId, CancellationToken ct)
    {
        await _editAgent.ReExecuteAsync(jobId, ct);
        return Ok();
    }
}
