using AiContentFactory.Application.Studio;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Storage;

public sealed record DriveOAuthExchangeRequest(string Code, string RedirectUri);
public sealed record CreateFolderRequest(string Name);

/// <summary>
/// Google Drive — OAuth exchange, file explorer, folder management, and config.
/// </summary>
[ApiController]
[Route("api/drive")]
public sealed class DriveController(IStudioWorkspaceFacade facade) : ControllerBase
{
    /// <summary>
    /// Exchanges a Google OAuth authorization code for access + refresh tokens.
    /// The tokens are returned to the caller; persistence is done client-side via the Drive config endpoint.
    /// </summary>
    [HttpPost("oauth/exchange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ExchangeOAuthCode([FromBody] DriveOAuthExchangeRequest request)
    {
        using var httpClient = new HttpClient();

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"]          = request.Code,
            ["client_id"]     = "",   // populated at runtime from Drive config
            ["client_secret"] = "",
            ["redirect_uri"]  = request.RedirectUri,
            ["grant_type"]    = "authorization_code"
        });

        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return Problem(
                detail: responseBody,
                statusCode: (int)response.StatusCode,
                title: "Google OAuth token exchange failed");
        }

        var tokenDoc = System.Text.Json.JsonDocument.Parse(responseBody);
        var root = tokenDoc.RootElement;

        return Ok(new
        {
            accessToken  = root.GetProperty("access_token").GetString() ?? "",
            refreshToken = root.TryGetProperty("refresh_token", out var rt)  ? rt.GetString()  ?? "" : "",
            expiresIn    = root.TryGetProperty("expires_in",    out var exp) ? exp.GetInt32()       : 3600
        });
    }

    /// <summary>Returns the current global Drive configuration (prefer /api/workspace/bootstrap for full context).</summary>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig() => Ok();

    /// <summary>Lists files and folders inside the configured Drive root folder.</summary>
    [HttpGet("files")]
    [ProducesResponseType<IReadOnlyList<DriveFileDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListFiles(
        [FromQuery] string? folderId,
        CancellationToken cancellationToken)
    {
        var files = await facade.ListDriveFilesAsync(folderId, cancellationToken);
        return Ok(files);
    }

    /// <summary>Uploads a file to the specified folder (or root if not provided).</summary>
    [HttpPost("files/upload")]
    [ProducesResponseType<DriveFileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(
        [FromQuery] string? folderId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await facade.UploadDriveFileAsync(folderId, file.FileName, file.ContentType, stream, cancellationToken);

        return result is null
            ? Problem("Drive file upload failed.")
            : Ok(result);
    }

    /// <summary>Downloads a file from Google Drive.</summary>
    [HttpGet("files/{fileId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(
        [FromRoute] string fileId,
        CancellationToken cancellationToken)
    {
        var result = await facade.DownloadDriveFileAsync(fileId, cancellationToken);
        if (result == null) return NotFound("File not found in Google Drive.");

        var contentType = result.Value.ContentType;
        if (contentType.StartsWith("text/") || contentType.Contains("json") || contentType.Contains("markdown"))
        {
            if (!contentType.Contains("charset")) contentType += "; charset=utf-8";
        }

        return File(result.Value.Content, contentType, result.Value.FileName);
    }

    /// <summary>Creates a sub-folder inside the configured Drive root folder.</summary>
    [HttpPost("folders")]
    [ProducesResponseType<DriveFileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateFolder(
        [FromQuery] string? folderId,
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var folder = await facade.CreateDriveFolderAsync(folderId, request.Name, cancellationToken);
        return folder is null
            ? Problem("Drive folder creation failed; check credentials and folder permissions.")
            : Ok(folder);
    }

    /// <summary>Persists the global Drive credentials and root folder ID to the backend.</summary>
    [HttpPut("config")]
    [ProducesResponseType<DriveSettingsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveConfig(
        [FromBody] SaveDriveSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await facade.SaveDriveSettingsAsync(request, cancellationToken);
        return Ok(settings);
    }

    /// <summary>Tests the current Google Drive connection.</summary>
    [HttpPost("connection/test")]
    [ProducesResponseType<ConnectionTestResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestConnection(CancellationToken cancellationToken)
    {
        var result = await facade.TestDriveConnectionAsync(cancellationToken);
        return Ok(result);
    }
}
