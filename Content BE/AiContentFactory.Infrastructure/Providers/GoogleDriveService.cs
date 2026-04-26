using System.Net.Http.Headers;
using System.Text.Json;
using AiContentFactory.Application.Studio;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class GoogleDriveService(HttpClient httpClient) : IGoogleDriveService
{
    public async Task<IReadOnlyList<DriveFileDto>> ListFilesAsync(DriveSettingsDto settings, string? folderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.RefreshToken))
        {
            return [];
        }

        // 1. Get Access Token
        var accessToken = await GetAccessTokenAsync(settings, cancellationToken);
        if (accessToken == null)
        {
            throw new InvalidOperationException("Failed to refresh Google access token. Check Drive client ID, client secret, and refresh token.");
        }

        // 2. List Files
        var folderToList = string.IsNullOrWhiteSpace(folderId) ? settings.RootFolderId : folderId;
        var query = string.IsNullOrWhiteSpace(folderToList) 
            ? "trashed = false" 
            : $"'{folderToList}' in parents and trashed = false";

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(query)}&fields=files(id,name,mimeType,createdTime,webViewLink)");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Google Drive list failed ({(int)response.StatusCode} {response.StatusCode}). {body}");
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var files = doc.RootElement.GetProperty("files");

        var result = new List<DriveFileDto>();
        foreach (var file in files.EnumerateArray())
        {
            var id = file.GetProperty("id").GetString() ?? "";
            var name = file.GetProperty("name").GetString() ?? "";
            var mimeType = file.GetProperty("mimeType").GetString() ?? "";
            var createdTime = file.TryGetProperty("createdTime", out var ct) ? ct.GetDateTimeOffset() : DateTimeOffset.UtcNow;
            
            // Map Drive files to the structure expected by the explorer
            string type = "file";
            if (mimeType.Contains("folder")) type = "folder";
            else if (mimeType.StartsWith("video/")) type = "video";
            else if (mimeType.StartsWith("image/")) type = "image";
            else if (mimeType.Contains("spreadsheet") || mimeType.Contains("sheet")) type = "spreadsheet";
            else if (mimeType.Contains("presentation") || mimeType.Contains("slides")) type = "presentation";
            else if (mimeType.Contains("pdf")) type = "pdf";
            else if (mimeType.Contains("google-apps") || mimeType.Contains("document")) type = "google-doc";

            result.Add(new DriveFileDto(
                id,
                name,
                type,
                "Unknown",
                createdTime.ToString("MMM dd")
            ));
        }

        return result;
    }

    public async Task<DriveFileDto?> CreateFolderAsync(DriveSettingsDto settings, string? folderId, string folderName, CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(settings, cancellationToken);
        if (accessToken == null) return null;

        var parentId = string.IsNullOrWhiteSpace(folderId) ? settings.RootFolderId : folderId;

        var metadata = new
        {
            name = folderName,
            mimeType = "application/vnd.google-apps.folder",
            parents = string.IsNullOrWhiteSpace(parentId) ? null : new[] { parentId }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/drive/v3/files");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(metadata), System.Text.Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var id = doc.RootElement.GetProperty("id").GetString() ?? "";

        return new DriveFileDto(id, folderName, "folder", "-", DateTimeOffset.UtcNow.ToString("MMM dd"));
    }

    public async Task<DriveFileDto?> UploadFileAsync(DriveSettingsDto settings, string? folderId, string fileName, string contentType, Stream fileStream, CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(settings, cancellationToken);
        if (accessToken == null) return null;

        var folderToUploadTo = string.IsNullOrWhiteSpace(folderId) ? settings.RootFolderId : folderId;
        
        var metadata = new
        {
            name = fileName,
            parents = string.IsNullOrWhiteSpace(folderToUploadTo) ? null : new[] { folderToUploadTo }
        };

        var content = new MultipartFormDataContent();
        var metadataContent = new StringContent(JsonSerializer.Serialize(metadata), System.Text.Encoding.UTF8, "application/json");
        metadataContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Add(metadataContent, "metadata");

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart&fields=id,name,mimeType,createdTime");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = content;

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var root = doc.RootElement;
        
        var id = root.GetProperty("id").GetString() ?? "";
        var name = root.GetProperty("name").GetString() ?? "";
        var mimeType = root.GetProperty("mimeType").GetString() ?? "";
        var createdTime = root.TryGetProperty("createdTime", out var ct) ? ct.GetDateTimeOffset() : DateTimeOffset.UtcNow;

        string type = "file";
        if (mimeType.Contains("folder")) type = "folder";
        else if (mimeType.StartsWith("video/")) type = "video";
        else if (mimeType.StartsWith("image/")) type = "image";
        else if (mimeType.Contains("google-apps")) type = "google-doc";

        return new DriveFileDto(id, name, type, "Unknown", createdTime.ToString("MMM dd"));
    }

    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadFileAsync(DriveSettingsDto settings, string fileId, CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(settings, cancellationToken);
        if (accessToken == null) return null;

        // 1. Get Metadata to get the filename and type
        var metaRequest = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/drive/v3/files/{fileId}?fields=name,mimeType");
        metaRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var metaResponse = await httpClient.SendAsync(metaRequest, cancellationToken);
        if (!metaResponse.IsSuccessStatusCode) return null;

        using var metaDoc = await JsonDocument.ParseAsync(await metaResponse.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var fileName = metaDoc.RootElement.GetProperty("name").GetString() ?? "download";
        var contentType = metaDoc.RootElement.GetProperty("mimeType").GetString() ?? "application/octet-stream";

        // Handle Google Workspace documents (must be exported)
        bool isGoogleDoc = contentType.StartsWith("application/vnd.google-apps.");
        string downloadUrl = $"https://www.googleapis.com/drive/v3/files/{fileId}?alt=media";

        if (isGoogleDoc)
        {
            var exportType = contentType switch
            {
                "application/vnd.google-apps.document" => ("application/pdf", ".pdf"),
                "application/vnd.google-apps.spreadsheet" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx"),
                "application/vnd.google-apps.presentation" => ("application/pdf", ".pdf"),
                "application/vnd.google-apps.script" => ("application/vnd.google-apps.script+json", ".json"),
                _ => ("application/pdf", ".pdf")
            };
            
            downloadUrl = $"https://www.googleapis.com/drive/v3/files/{fileId}/export?mimeType={Uri.EscapeDataString(exportType.Item1)}";
            contentType = exportType.Item1;
            if (!fileName.EndsWith(exportType.Item2, StringComparison.OrdinalIgnoreCase)) fileName += exportType.Item2;
        }
        else if (!fileName.Contains('.') && !string.IsNullOrEmpty(contentType))
        {
            var ext = contentType switch
            {
                "video/mp4" => ".mp4",
                "video/quicktime" => ".mov",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "application/pdf" => ".pdf",
                "application/json" => ".json",
                "application/vnd.google-makersuite.prompt" => ".json",
                "text/markdown" => ".md",
                "text/plain" => ".txt",
                _ => ""
            };
            fileName += ext;
        }

        // 2. Download Content
        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return (stream, contentType, fileName);
    }

    public async Task<string> WatchFolderAsync(DriveSettingsDto settings, string folderId, string webhookUrl, CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(settings, cancellationToken);
        if (accessToken == null) return "Error: Failed to obtain access token.";

        var body = new
        {
            id = Guid.NewGuid().ToString(),
            type = "web_hook",
            address = webhookUrl
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/drive/v3/files/{folderId}/watch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return $"Error: {response.StatusCode} - {error}";
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        return doc.RootElement.GetProperty("resourceId").GetString() ?? "Success";
    }

    private async Task<string?> GetAccessTokenAsync(DriveSettingsDto settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.RefreshToken))
            return null;

        var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["refresh_token"] = settings.RefreshToken,
            ["grant_type"] = "refresh_token"
        }), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode) return null;

        using var tokenDoc = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        return tokenDoc.RootElement.GetProperty("access_token").GetString();
    }
}
