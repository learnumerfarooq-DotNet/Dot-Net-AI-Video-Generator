using AiContentFactory.Infrastructure.Publishing.YouTube;
using Microsoft.AspNetCore.Mvc;


namespace AiContentFactory.Api.Controllers.Settings;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    [HttpGet("agents")]
    public IActionResult GetAllAgentSettings() => Ok(new List<object>());

    [HttpGet("agents/{key}")]
    public IActionResult GetAgentSettings(string key) => Ok(new { agentKey = key });

    [HttpPut("agents/{key}")]
    public IActionResult SaveAgentSettings(string key, [FromBody] object settings) => Ok(settings);

    [HttpPost("agents/{key}/reset")]
    public IActionResult ResetAgentSettings(string key) => Ok();

    [HttpPost("agents/{key}/test")]
    public IActionResult TestAgentConnection(string key) => Ok(new { success = true });

    [HttpGet("global")]
    public IActionResult GetGlobalSettings() => Ok(new { });

    [HttpPut("global")]
    public IActionResult SaveGlobalSettings([FromBody] object settings) => Ok(settings);

    [HttpGet("youtube/auth-url")]
    public async Task<IActionResult> GetYouTubeAuthUrl(
        [FromServices] YouTubeOAuthManager oauth,
        [FromQuery] string agentKey,
        [FromQuery] string redirectUri)
    {
        var url = await oauth.GetAuthorizationUrlAsync(agentKey, redirectUri);
        return Ok(url);
    }
}

