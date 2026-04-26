using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Providers;

/// <summary>
/// Per-agent settings — provider, model, credentials, and storage configuration.
/// </summary>
[ApiController]
[Route("api/settings")]
public sealed class SettingsController(IStudioWorkspaceFacade facade) : ControllerBase
{
    /// <summary>Persists provider and credential settings for a single agent.</summary>
    [HttpPut("agents/{agentKey}")]
    [ProducesResponseType<AgentSettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveAgentSettings(
        [FromRoute] string agentKey,
        [FromBody] SaveAgentSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await facade.SaveAgentSettingsAsync(agentKey, request, cancellationToken);
        return settings is null ? NotFound() : Ok(settings);
    }
}
