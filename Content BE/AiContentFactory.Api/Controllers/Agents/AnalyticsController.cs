using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsAgent _analyticsAgent;

    public AnalyticsController(IAnalyticsAgent analyticsAgent)
    {
        _analyticsAgent = analyticsAgent;
    }

    [HttpPost("collect")]
    public async Task<ActionResult<AnalyticsReport>> CollectAnalytics(CancellationToken ct)
    {
        var report = await _analyticsAgent.CollectDailyAnalyticsAsync(ct);
        return Ok(report);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<AnalyticsReport>> GetLatestReport(CancellationToken ct)
    {
        var report = await _analyticsAgent.GetLatestReportAsync(ct);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<AnalyticsReport>>> GetReportHistory([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var history = await _analyticsAgent.GetReportHistoryAsync(days, ct);
        return Ok(history);
    }

    [HttpGet("video/{videoId}/score")]
    public async Task<ActionResult<double>> GetVideoScore(Guid videoId, CancellationToken ct)
    {
        var score = await _analyticsAgent.CalculateContentScoreAsync(videoId, ct);
        return Ok(score);
    }
}
