using System.Net.Http.Headers;
using System.Text.Json;
using AiContentFactory.Application.Studio;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class GoogleDriveService(HttpClient httpClient) : IGoogleDriveService
{
    public async Task<IReadOnlyList<DriveFileDto>> ListFilesAsync(DriveSettingsDto settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.RefreshToken))
        {
            return [];
        }

        // 1. Get Access Token
        var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["refresh_token"] = settings.RefreshToken,
            ["grant_type"] = "refresh_token"
        }), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return [];
        }

        var tokenDoc = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

        // 2. List Files
        var query = string.IsNullOrWhiteSpace(settings.RootFolderId) 
            ? "trashed = false" 
            : $"'{settings.RootFolderId}' in parents and trashed = false";

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(query)}&fields=files(id,name,mimeType,createdTime,webViewLink)");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var files = doc.RootElement.GetProperty("files");

        var result = new List<DriveFileDto>();
        foreach (var file in files.EnumerateArray())
        {
            var id = file.GetProperty("id").GetString() ?? "";
            var name = file.GetProperty("name").GetString() ?? "";
            var mimeType = file.GetProperty("mimeType").GetString() ?? "";
            var createdTime = file.TryGetProperty("createdTime", out var ct) ? ct.GetDateTimeOffset() : DateTimeOffset.UtcNow;
            
            // Map Drive files to the structure expected by the explorer
            result.Add(new DriveFileDto(
                id,
                name,
                mimeType.Contains("folder") ? "folder" : "video",
                "Unknown", // API would need more fields for size
                createdTime.ToString("MMM dd")
            ));
        }

        return result;
    }

    public async Task<DriveFileDto?> CreateFolderAsync(DriveSettingsDto settings, string folderName, CancellationToken cancellationToken)
    {
        // 1. Get Access Token
        var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["refresh_token"] = settings.RefreshToken,
            ["grant_type"] = "refresh_token"
        }), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode) return null;

        var tokenDoc = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

        // 2. Create Folder
        var metadata = new
        {
            name = folderName,
            mimeType = "application/vnd.google-apps.folder",
            parents = string.IsNullOrWhiteSpace(settings.RootFolderId) ? null : new[] { settings.RootFolderId }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/drive/v3/files");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(metadata), System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        var id = doc.RootElement.GetProperty("id").GetString() ?? "";

        return new DriveFileDto(id, folderName, "folder", "-", DateTimeOffset.UtcNow.ToString("MMM dd"));
    }
}
