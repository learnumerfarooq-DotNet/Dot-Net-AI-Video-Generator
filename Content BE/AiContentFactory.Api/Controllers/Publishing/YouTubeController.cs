using AiContentFactory.Domain.Publishing.YouTube;
using AiContentFactory.Infrastructure.Publishing.YouTube;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Publishing;

[ApiController]
[Route("api/publish/youtube")]
public class YouTubeController : ControllerBase
{
    private readonly YouTubeUploadService _uploadService;
    private readonly YouTubeOAuthManager _oauthManager;
    private readonly YouTubeAnalyticsService _analyticsService;

    public YouTubeController(
        YouTubeUploadService uploadService,
        YouTubeOAuthManager oauthManager,
        YouTubeAnalyticsService analyticsService)
    {
        _uploadService = uploadService;
        _oauthManager = oauthManager;
        _analyticsService = analyticsService;
    }

    [HttpPost("auth/url")]
    public async Task<ActionResult<string>> GetAuthUrl([FromQuery] string agentKey, [FromQuery] string redirectUri)
    {
        var url = await _oauthManager.GetAuthorizationUrlAsync(agentKey, redirectUri);
        return Ok(url);
    }

    [HttpGet("{videoId}/status")]
    public async Task<ActionResult<string>> GetStatus(string videoId, [FromQuery] string agentKey, CancellationToken ct)
    {
        var status = await _uploadService.GetVideoStatusAsync(videoId, agentKey, ct);
        return Ok(status);
    }

    [HttpGet("analytics/{videoId}")]
    public async Task<ActionResult> GetAnalytics(string videoId, [FromQuery] string agentKey, CancellationToken ct)
    {
        var stats = await _analyticsService.GetVideoStatsAsync(videoId, agentKey, ct);
        return Ok(stats);
    }
}
