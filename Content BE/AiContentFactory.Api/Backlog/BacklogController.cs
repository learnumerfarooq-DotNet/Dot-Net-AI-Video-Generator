using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Backlog;

/// <summary>
/// Video backlog / pipeline stage management.
/// </summary>
[ApiController]
[Route("api/videos")]
public sealed class BacklogController(IStudioWorkspaceFacade facade) : ControllerBase
{
    /// <summary>Moves a video item to a different stage in the production pipeline.</summary>
    [HttpPost("{id:guid}/stage")]
    [ProducesResponseType<VideoItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStage(
        [FromRoute] Guid id,
        [FromBody] UpdateVideoStageRequest request,
        CancellationToken cancellationToken)
    {
        var video = await facade.UpdateVideoStageAsync(id, request, cancellationToken);
        return video is null ? NotFound() : Ok(video);
    }
}
