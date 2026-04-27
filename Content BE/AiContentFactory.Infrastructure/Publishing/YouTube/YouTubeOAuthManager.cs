using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing.YouTube;
using AiContentFactory.Infrastructure.Persistence;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Publishing.YouTube;

public class YouTubeOAuthManager
{
    private readonly YouTubeOptions _options;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<YouTubeOAuthManager> _logger;

    public YouTubeOAuthManager(
        IOptions<YouTubeOptions> options,
        StudioDbContext dbContext,
        ILogger<YouTubeOAuthManager> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(string agentKey, CancellationToken ct)
    {
        var credential = await GetCredentialAsync(agentKey, ct);
        if (credential.Token.IsStale)
        {
            await credential.RefreshTokenAsync(ct);
            await SaveGoogleCredentialAsync(agentKey, credential, ct);
        }
        return credential.Token.AccessToken;
    }

    public async Task<UserCredential> GetCredentialAsync(string agentKey, CancellationToken ct)
    {
        var dbCred = await _dbContext.YouTubeCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                     ?? throw new Exception($"No YouTube credentials found for {agentKey}");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = _options.Scopes,
            DataStore = new NullDataStore()
        });

        var token = new TokenResponse
        {
            AccessToken = dbCred.AccessToken,
            RefreshToken = dbCred.RefreshToken,
            ExpiresInSeconds = (long)(dbCred.TokenExpiresAt - DateTimeOffset.UtcNow).TotalSeconds
        };

        return new UserCredential(flow, agentKey, token);
    }

    public async Task SaveCredentialAsync(string agentKey, YouTubeCredential cred, CancellationToken ct)
    {
        var existing = await _dbContext.YouTubeCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct);
        if (existing != null)
        {
            existing.AccessToken = cred.AccessToken;
            existing.RefreshToken = cred.RefreshToken;
            existing.TokenExpiresAt = cred.TokenExpiresAt;
            existing.ChannelId = cred.ChannelId;
        }
        else
        {
            cred.AgentKey = agentKey;
            _dbContext.YouTubeCredentials.Add(cred);
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task SaveGoogleCredentialAsync(string agentKey, UserCredential credential, CancellationToken ct)
    {
        var dbCred = await _dbContext.YouTubeCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct);
        if (dbCred != null)
        {
            dbCred.AccessToken = credential.Token.AccessToken;
            if (!string.IsNullOrEmpty(credential.Token.RefreshToken))
            {
                dbCred.RefreshToken = credential.Token.RefreshToken;
            }
            dbCred.TokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ValidateCredentialAsync(string agentKey, CancellationToken ct)
    {
        try
        {
            await GetAccessTokenAsync(agentKey, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetAuthorizationUrlAsync(string agentKey, string redirectUri)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = _options.Scopes
        });

        var result = new GoogleAuthorizationCodeRequestUrl(new Uri(flow.AuthorizationServerUrl))
        {
            ClientId = flow.ClientSecrets.ClientId,
            RedirectUri = redirectUri,
            Scope = string.Join(" ", flow.Scopes),
            State = agentKey,
            AccessType = "offline",
            Prompt = "consent"
        };
        
        return result.Build().ToString();
    }
}

public class NullDataStore : IDataStore
{
    public Task ClearAsync() => Task.CompletedTask;
    public Task DeleteAsync<T>(string key) => Task.CompletedTask;
    public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T)!);
    public Task StoreAsync<T>(string key, T value) => Task.CompletedTask;
}
