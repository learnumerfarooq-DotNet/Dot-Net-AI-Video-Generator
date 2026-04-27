using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing.TikTok;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.TikTok;

public class TikTokOAuthManager
{
    private readonly TikTokOptions _options;
    private readonly StudioDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TikTokOAuthManager> _logger;

    public TikTokOAuthManager(
        IOptions<TikTokOptions> options,
        StudioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILogger<TikTokOAuthManager> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(string agentKey, CancellationToken ct)
    {
        var cred = await _dbContext.TikTokCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception($"TikTok credentials not found for {agentKey}");

        if (cred.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return await RefreshAccessTokenAsync(agentKey, ct);
        }

        return cred.AccessToken;
    }

    public async Task<string> RefreshAccessTokenAsync(string agentKey, CancellationToken ct)
    {
        var cred = await _dbContext.TikTokCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception($"TikTok credentials not found for {agentKey}");

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://open.tiktokapis.com/v2/auth/token/");
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_key", _options.ClientKey },
            { "client_secret", _options.ClientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", cred.RefreshToken }
        });

        request.Content = content;
        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TikTokTokenResponse>(cancellationToken: ct);
        if (result == null || string.IsNullOrEmpty(result.AccessToken)) throw new Exception("Failed to refresh TikTok token");

        cred.AccessToken = result.AccessToken;
        cred.RefreshToken = result.RefreshToken ?? cred.RefreshToken;
        cred.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn);

        await _dbContext.SaveChangesAsync(ct);
        return cred.AccessToken;
    }

    public async Task<string> GetAuthorizationUrl(string agentKey, string state)
    {
        var url = $"https://www.tiktok.com/v2/auth/authorize/" +
                  $"?client_key={_options.ClientKey}" +
                  $"&scope={_options.Scopes}" +
                  $"&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                  $"&state={state}";
        return url;
    }

    private class TikTokTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string OpenId { get; set; } = string.Empty;
    }
}
