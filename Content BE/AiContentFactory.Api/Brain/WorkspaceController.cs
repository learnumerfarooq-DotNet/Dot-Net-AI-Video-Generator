using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Brain;

/// <summary>
/// Workspace bootstrap — returns the full studio state in a single call.
/// </summary>
[ApiController]
[Route("api/workspace")]
public sealed class WorkspaceController(IStudioWorkspaceFacade facade) : ControllerBase
{
    [HttpGet("bootstrap")]
    [ProducesResponseType<WorkspaceBootstrapResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBootstrap(CancellationToken cancellationToken)
    {
        var response = await facade.GetBootstrapAsync(cancellationToken);
        return Ok(response);
    }
}
