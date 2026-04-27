using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Drive;

[ApiController]
[Route("api/drive")]
public class DriveController : ControllerBase
{
    private readonly IGoogleDriveService _driveService;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly IStudioWorkspaceStore _workspaceStore;

    public DriveController(
        IGoogleDriveService driveService,
        IPipelineOrchestrator orchestrator,
        IStudioWorkspaceStore workspaceStore)
    {
        _driveService = driveService;
        _orchestrator = orchestrator;
        _workspaceStore = workspaceStore;
    }

    [HttpGet("files")]
    public async Task<IActionResult> ListFiles([FromQuery] string? folderId, CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return BadRequest("Drive settings not configured.");

        var files = await _driveService.ListFilesAsync(settings, folderId, ct);
        return Ok(files);
    }

    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request, CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return BadRequest("Drive settings not configured.");

        var folder = await _driveService.CreateFolderAsync(settings, request.ParentId, request.Name, ct);
        return Ok(folder);
    }

    [HttpPost("files/upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromQuery] string? folderId, CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return BadRequest("Drive settings not configured.");

        using var stream = file.OpenReadStream();
        var uploaded = await _driveService.UploadFileAsync(settings, folderId, file.FileName, file.ContentType, stream, ct);
        return Ok(uploaded);
    }

    [HttpGet("files/{fileId}/download")]
    public async Task<IActionResult> DownloadFile(string fileId, CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return BadRequest("Drive settings not configured.");

        var result = await _driveService.DownloadFileAsync(settings, fileId, ct);
        if (result == null) return NotFound();

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    [HttpPost("connection/test")]
    public async Task<IActionResult> TestConnection(CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return Ok(new { success = false, message = "Drive settings not configured." });

        try
        {
            await _driveService.ListFilesAsync(settings, null, ct);
            return Ok(new { success = true, message = "Connection successful." });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = "Connection failed.", details = ex.Message });
        }
    }

    [HttpPost("pipeline/start")]
    public async Task<IActionResult> StartPipeline([FromBody] StartPipelineRequest request, CancellationToken ct)
    {
        var job = await _orchestrator.StartPipelineAsync(request.FileId, request.FileName, ct);
        return Ok(new { jobId = job.Id, status = job.Status });
    }

    [HttpGet("mapping")]
    public async Task<IActionResult> GetFolderMapping(CancellationToken ct)
    {
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        return Ok(settings); // Simplified mapping for now
    }

    [HttpPost("oauth/exchange")]
    public IActionResult OAuthExchangeCode([FromBody] OAuthExchangeRequest request) => Ok(new { accessToken = "at", refreshToken = "rt", expiresIn = 3600 });
}

public record CreateFolderRequest(string Name, string? ParentId);
public record StartPipelineRequest(string FileId, string FileName);
public record OAuthExchangeRequest(string Code, string RedirectUri);
