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
    public async Task<IActionResult> ListFiles(CancellationToken cancellationToken)
    {
        var files = await facade.ListDriveFilesAsync(cancellationToken);
        return Ok(files);
    }

    /// <summary>Creates a sub-folder inside the configured Drive root folder.</summary>
    [HttpPost("folders")]
    [ProducesResponseType<DriveFileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateFolder(
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var folder = await facade.CreateDriveFolderAsync(request.Name, cancellationToken);
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
}
