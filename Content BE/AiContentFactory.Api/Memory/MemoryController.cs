using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Memory;

/// <summary>
/// Memory review-queue and suggestion endpoints.
/// </summary>
[ApiController]
[Route("api/memory")]
public sealed class MemoryController(IStudioWorkspaceFacade facade) : ControllerBase
{
    /// <summary>Approves a pending memory record, optionally applying the caller's revisions.</summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<MemoryRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        [FromBody] ReviewMemoryRequest request,
        CancellationToken cancellationToken)
    {
        var memory = await facade.ApproveMemoryAsync(id, request, cancellationToken);
        return memory is null ? NotFound() : Ok(memory);
    }

    /// <summary>Rejects a pending memory record, optionally applying the caller's revisions.</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType<MemoryRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id,
        [FromBody] ReviewMemoryRequest request,
        CancellationToken cancellationToken)
    {
        var memory = await facade.RejectMemoryAsync(id, request, cancellationToken);
        return memory is null ? NotFound() : Ok(memory);
    }

    /// <summary>Returns all memory records currently in the pending-suggestion queue.</summary>
    [HttpGet("suggestions/pending")]
    [ProducesResponseType<IReadOnlyList<MemorySuggestionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingSuggestions(CancellationToken cancellationToken)
    {
        var suggestions = await facade.GetPendingMemorySuggestionsAsync(cancellationToken);
        return Ok(suggestions);
    }
}
