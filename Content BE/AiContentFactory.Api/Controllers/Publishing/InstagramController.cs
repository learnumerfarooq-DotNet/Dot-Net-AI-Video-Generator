using AiContentFactory.Infrastructure.Publishing.Instagram;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Publishing;

[ApiController]
[Route("api/publish/instagram")]
public class InstagramController : ControllerBase
{
    private readonly InstagramOAuthManager _oauthManager;

    public InstagramController(InstagramOAuthManager oauthManager)
    {
        _oauthManager = oauthManager;
    }

    [HttpPost("auth/url")]
    public ActionResult<string> GetAuthUrl([FromQuery] string agentKey, [FromQuery] string state)
    {
        var url = _oauthManager.GetAuthorizationUrl(agentKey, state);
        return Ok(url);
    }
}
