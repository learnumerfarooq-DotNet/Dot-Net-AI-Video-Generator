using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Dashboard;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController(IStudioWorkspaceFacade facade) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardWorkspaceDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await facade.GetDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("videos/{stage}")]
    public async Task<ActionResult<PaginatedListDto<VideoItemDto>>> GetVideos(
        string stage, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var result = await facade.GetVideosByStageAsync(stage, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("runs")]
    public async Task<ActionResult<PaginatedListDto<AgentRunDto>>> GetRuns(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var result = await facade.GetAgentRunsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
