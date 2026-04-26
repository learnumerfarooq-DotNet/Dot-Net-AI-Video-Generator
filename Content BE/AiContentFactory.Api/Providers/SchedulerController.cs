using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Providers;

/// <summary>
/// Scheduler endpoints — manual job creation.
/// </summary>
[ApiController]
[Route("api/scheduler")]
public sealed class SchedulerController(IStudioWorkspaceFacade facade) : ControllerBase
{
    /// <summary>Creates a one-off manual schedule entry for any agent task.</summary>
    [HttpPost("manual")]
    [ProducesResponseType<ScheduleJobDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateManual(
        [FromBody] CreateManualScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await facade.CreateManualScheduleAsync(request, cancellationToken);
        return Ok(schedule);
    }
}
