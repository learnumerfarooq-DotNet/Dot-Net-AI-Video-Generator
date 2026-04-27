using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Trends;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/trend")]
public class TrendsController : ControllerBase
{
    private readonly ITrendAgent _trendAgent;

    public TrendsController(ITrendAgent trendAgent)
    {
        _trendAgent = trendAgent;
    }

    [HttpPost("discover")]
    public async Task<ActionResult<TrendResult>> DiscoverTrends(CancellationToken ct)
    {
        var result = await _trendAgent.DiscoverTrendsAsync(ct);
        return Ok(result);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<TrendResult>> GetLatestTrend(CancellationToken ct)
    {
        var result = await _trendAgent.GetLatestTrendResultAsync(ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<TrendResult>>> GetTrendHistory([FromQuery] int days = 7, CancellationToken ct = default)
    {
        var history = await _trendAgent.GetTrendHistoryAsync(days, ct);
        return Ok(history);
    }
}
