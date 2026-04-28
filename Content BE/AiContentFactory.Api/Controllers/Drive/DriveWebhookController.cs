using AiContentFactory.Application.Common;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Events;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Api.Controllers.Drive;

[ApiController]
[Route("api/webhooks/drive")]
public sealed class DriveWebhookController(
    IStudioWorkspaceStore store,
    IGoogleDriveService driveService,
    IRealtimeEventEmitter emitter,
    ILogger<DriveWebhookController> logger) : ControllerBase
{
    [HttpPost("notify")]
    public async Task<IActionResult> Notify(CancellationToken cancellationToken)
    {
        // Google Drive sends resource ID in headers
        var resourceId = Request.Headers["X-Goog-Resource-ID"].ToString();
        var state = Request.Headers["X-Goog-Resource-State"].ToString(); // "update", "trash", "delete", etc.
        var channelId = Request.Headers["X-Goog-Channel-ID"].ToString();

        logger.LogInformation("Drive Webhook received: Resource={ResourceId}, State={State}, Channel={ChannelId}", resourceId, state, channelId);

        if (state == "sync")
        {
            // Initial sync notification, ignore
            return Ok();
        }

        // Trigger a background sync of the assets
        // In a real high-scale system, we would queue a specific job for this ResourceId.
        // For our MVP, we can trigger the same logic as the background worker but on-demand.
        
        try
        {
            // We fetch settings to know which folder to sync (or just sync everything)
            var settings = await store.GetDriveSettingsAsync(cancellationToken);
            
            // For now, we reuse the existing ListFiles logic which effectively "syncs" the DB.
            // Note: A more efficient way would be to fetch ONLY the changed resource.
            var files = await driveService.ListFilesAsync(settings, null, cancellationToken);
            
            // Notify the UI that assets might have changed
            await emitter.EmitDriveFileDetectedAsync(new DriveFileDetectedPayload(resourceId, "Webhook Update", "Multiple"), cancellationToken);
            
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Drive Webhook.");
            return StatusCode(500);
        }
    }
}
