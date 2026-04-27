using AiContentFactory.Application.Errors;
using AiContentFactory.Domain.Errors;
using AiContentFactory.Domain.GlobalMemory;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers;

[ApiController]
[Route("api/errors")]
public class ErrorHandlingController : ControllerBase
{
    private readonly IErrorHandlingService _errorService;

    public ErrorHandlingController(IErrorHandlingService errorService)
    {
        _errorService = errorService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ErrorSummary>> GetSummary(CancellationToken ct)
    {
        return Ok(await _errorService.GetErrorSummaryAsync(ct));
    }

    [HttpGet("dead-letter")]
    public async Task<ActionResult<List<DeadLetterEntry>>> GetDeadLetterQueue(CancellationToken ct)
    {
        return Ok(await _errorService.GetDeadLetterQueueAsync(ct));
    }

    [HttpPost("dead-letter/{id}/resolve")]
    public async Task<IActionResult> ResolveDeadLetter(Guid id, [FromBody] string resolution, CancellationToken ct)
    {
        await _errorService.ResolveDeadLetterAsync(id, resolution, ct);
        return Ok();
    }

    [HttpPost("circuit/{agentKey}/reset")]
    public async Task<IActionResult> ResetCircuit(string agentKey, CancellationToken ct)
    {
        await _errorService.CloseCircuitBreakerAsync(agentKey, ct);
        return Ok();
    }

    [HttpGet("circuit/{agentKey}")]
    public async Task<ActionResult<CircuitBreakerState>> GetCircuitState(string agentKey, CancellationToken ct)
    {
        return Ok(await _errorService.GetCircuitStateAsync(agentKey, ct));
    }
}
