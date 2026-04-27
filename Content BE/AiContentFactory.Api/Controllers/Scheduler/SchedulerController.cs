using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Scheduler;

[ApiController]
[Route("api/scheduler")]
public class SchedulerController : ControllerBase
{
    [HttpGet("manual")]
    public IActionResult GetManualSchedules() => Ok(new List<object>());

    [HttpPost("manual")]
    public IActionResult CreateManualSchedule([FromBody] object draft) => Ok(draft);

    [HttpPut("{id}")]
    public IActionResult UpdateSchedule(string id, [FromBody] object updates) => Ok();

    [HttpDelete("{id}")]
    public IActionResult DeleteSchedule(string id) => Ok();

    [HttpPost("{id}/toggle")]
    public IActionResult ToggleSchedule(string id) => Ok();

    [HttpPost("{id}/run-now")]
    public IActionResult RunNow(string id) => Ok();

    [HttpGet("daily")]
    public IActionResult GetDailySchedule() => Ok(new List<object>());

    [HttpGet("retry")]
    public IActionResult GetRetryQueue() => Ok(new List<object>());

    [HttpPost("retry/{jobId}/now")]
    public IActionResult RetryNow(string jobId) => Ok();

    [HttpGet("queue")]
    public IActionResult GetQueueStats() => Ok(new { });

    [HttpGet("dead-letter")]
    public IActionResult GetDeadLetterQueue() => Ok(new List<object>());
}
