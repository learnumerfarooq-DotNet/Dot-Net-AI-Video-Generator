using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Brain;

/// <summary>
/// Workspace bootstrap — returns the full studio state in a single call.
/// </summary>
[ApiController]
[Route("api/workspace")]
public sealed class WorkspaceController : ControllerBase
{
    private readonly IStudioWorkspaceFacade _facade;

    public WorkspaceController(IStudioWorkspaceFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("bootstrap")]
    public async Task<ActionResult<WorkspaceBootstrapResponse>> GetBootstrap(CancellationToken cancellationToken)
    {
        return Ok(await _facade.GetBootstrapAsync(cancellationToken));
    }
}
