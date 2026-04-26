using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Api.Agents;

/// <summary>
/// Agent chat and management endpoints.
/// </summary>
[ApiController]
[Route("api/agents")]
public sealed class AgentsController(IStudioWorkspaceFacade facade, StudioDbContext db) : ControllerBase
{
    /// <summary>Sends a message to the specified agent and returns the reply.</summary>
    [HttpPost("{agentKey}/chat")]
    [ProducesResponseType<AgentChatResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage(
        [FromRoute] string agentKey,
        [FromBody] SendAgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await facade.SendAgentMessageAsync(agentKey, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Streams a message to the specified agent using SSE.</summary>
    [HttpGet("{agentKey}/chat/stream")]
    public async Task StreamMessage(
        [FromRoute] string agentKey,
        [FromQuery] string message,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var request = new SendAgentMessageRequest(message);
        await foreach (var chunk in facade.StreamAgentMessageAsync(agentKey, request, cancellationToken))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(chunk, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            });
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Removes broken/debug messages from an agent's chat history.
    /// Targets messages that begin with known error-prefix strings.
    /// </summary>
    [HttpDelete("{agentKey}/chat/cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupChat(
        [FromRoute] string agentKey,
        CancellationToken cancellationToken)
    {
        var broken = db.ChatMessages
            .Where(m => m.AgentKey == agentKey && (
                m.Content.StartsWith("No response content generated") ||
                m.Content.StartsWith("Main Brain response")));

        db.ChatMessages.RemoveRange(broken);
        var count = await db.SaveChangesAsync(cancellationToken);

        return Ok(new { deleted = count, agentKey });
    }

    /// <summary>Tests the connection to the agent's configured provider.</summary>
    [HttpPost("{agentKey}/connection/test")]
    [ProducesResponseType<ConnectionTestResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestConnection(
        [FromRoute] string agentKey,
        CancellationToken cancellationToken)
    {
        var result = await facade.TestAgentConnectionAsync(agentKey, cancellationToken);
        return Ok(result);
    }
}
