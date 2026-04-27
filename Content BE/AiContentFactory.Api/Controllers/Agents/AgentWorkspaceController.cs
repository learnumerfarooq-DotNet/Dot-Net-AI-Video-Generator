using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/{key}")]
public class AgentWorkspaceController : ControllerBase
{
    [HttpPost("run")]
    public IActionResult StartRun(string key) => Ok(new { runId = Guid.NewGuid().ToString() });

    [HttpPost("stop")]
    public IActionResult StopRun(string key) => Ok();

    [HttpPost("chat")]
    public IActionResult SendChat(string key, [FromBody] ChatRequest request) => 
        Ok(new { response = $"Acknowledged by {key}: {request.Message}" });

    [HttpDelete("chat/cleanup")]
    public IActionResult ClearChat(string key) => Ok();

    [HttpGet("runs")]
    public IActionResult GetRuns(string key, [FromQuery] int limit = 20) => Ok(new List<object>());

    [HttpGet("active-job")]
    public IActionResult GetActiveJob(string key) => Ok(null);

    [HttpGet("errors")]
    public IActionResult GetErrors(string key, [FromQuery] int limit = 5) => Ok(new List<object>());
}

public class ChatRequest { public string Message { get; set; } = string.Empty; }
