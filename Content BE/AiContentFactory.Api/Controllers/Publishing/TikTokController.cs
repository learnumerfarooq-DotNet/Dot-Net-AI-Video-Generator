using AiContentFactory.Infrastructure.Publishing.TikTok;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Publishing;

[ApiController]
[Route("api/publish/tiktok")]
public class TikTokController : ControllerBase
{
    private readonly TikTokOAuthManager _oauthManager;

    public TikTokController(TikTokOAuthManager oauthManager)
    {
        _oauthManager = oauthManager;
    }

    [HttpPost("auth/url")]
    public async Task<ActionResult<string>> GetAuthUrl([FromQuery] string agentKey, [FromQuery] string state)
    {
        var url = await _oauthManager.GetAuthorizationUrl(agentKey, state);
        return Ok(url);
    }
}
