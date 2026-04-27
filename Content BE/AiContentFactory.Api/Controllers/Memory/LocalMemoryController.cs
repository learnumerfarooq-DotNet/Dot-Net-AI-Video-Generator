using AiContentFactory.Application.Memory;
using AiContentFactory.Domain.Memory;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Memory;

[ApiController]
[Route("api/memory/local")]
public class LocalMemoryController : ControllerBase
{
    private readonly ILocalMemoryService _localMemoryService;

    public LocalMemoryController(ILocalMemoryService localMemoryService)
    {
        _localMemoryService = localMemoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentLocalMemory>>> GetAllLocalMemories(CancellationToken ct)
    {
        var memories = await _localMemoryService.GetAllAsync(ct);
        return Ok(memories);
    }

    [HttpGet("{agentKey}")]
    public async Task<ActionResult<AgentLocalMemory>> GetAgentMemory(string agentKey, CancellationToken ct)
    {
        var memory = await _localMemoryService.GetAsync(agentKey, ct);
        if (memory == null) return NotFound();
        return Ok(memory);
    }

    [HttpPut("{agentKey}")]
    public async Task<IActionResult> UpdateAgentMemory(string agentKey, [FromBody] object config, CancellationToken ct)
    {
        // For simplicity, we just save it as a generic object, it will serialize to JSON
        await _localMemoryService.SaveConfigAsync(agentKey, config, ct);
        return Ok();
    }

    [HttpPost("{agentKey}/reset")]
    public async Task<IActionResult> ResetAgentMemory(string agentKey, CancellationToken ct)
    {
        await _localMemoryService.ResetAsync(agentKey, ct);
        return Ok();
    }

    [HttpPost("{agentKey}/sync")]
    public async Task<IActionResult> SyncAgentMemoryToDrive(string agentKey, CancellationToken ct)
    {
        await _localMemoryService.SyncToDriveAsync(agentKey, ct);
        return Ok();
    }

    [HttpGet("{agentKey}/stats")]
    public async Task<ActionResult> GetAgentStats(string agentKey, CancellationToken ct)
    {
        var memory = await _localMemoryService.GetAsync(agentKey, ct);
        if (memory == null) return NotFound();
        
        return Ok(new
        {
            memory.RunCount,
            memory.SuccessCount,
            memory.FailureCount,
            memory.LastRunAt,
            memory.LastSuccessAt,
            memory.LastErrorAt,
            memory.LastErrorMessage
        });
    }
}
