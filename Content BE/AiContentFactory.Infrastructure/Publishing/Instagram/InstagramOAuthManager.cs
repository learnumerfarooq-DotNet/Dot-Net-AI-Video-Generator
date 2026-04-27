using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing.Instagram;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.Instagram;

public class InstagramOAuthManager
{
    private readonly InstagramOptions _options;
    private readonly StudioDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InstagramOAuthManager> _logger;

    public InstagramOAuthManager(
        IOptions<InstagramOptions> options,
        StudioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILogger<InstagramOAuthManager> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(string agentKey, CancellationToken ct)
    {
        var cred = await _dbContext.InstagramCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception($"Instagram credentials not found for {agentKey}");

        if (cred.ExpiresAt <= DateTimeOffset.UtcNow.AddDays(1))
        {
            return await RefreshLongLivedTokenAsync(agentKey, ct);
        }

        return cred.AccessToken;
    }

    public async Task<string> RefreshLongLivedTokenAsync(string agentKey, CancellationToken ct)
    {
        var cred = await _dbContext.InstagramCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception($"Instagram credentials not found for {agentKey}");

        var client = _httpClientFactory.CreateClient();
        var url = $"{_options.GraphApiBaseUrl}/oauth/access_token?" +
                  $"grant_type=fb_exchange_token&" +
                  $"client_id={_options.AppId}&" +
                  $"client_secret={_options.AppSecret}&" +
                  $"fb_exchange_token={cred.AccessToken}";

        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MetaTokenResponse>(cancellationToken: ct);
        if (result == null || string.IsNullOrEmpty(result.AccessToken)) throw new Exception("Failed to refresh Instagram long-lived token");

        cred.AccessToken = result.AccessToken;
        cred.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn);

        await _dbContext.SaveChangesAsync(ct);
        return cred.AccessToken;
    }

    public string GetAuthorizationUrl(string agentKey, string state)
    {
        return $"https://www.facebook.com/v20.0/dialog/oauth?" +
               $"client_id={_options.AppId}&" +
               $"redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}&" +
               $"scope={_options.Scopes}&" +
               $"state={state}";
    }

    private class MetaTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
