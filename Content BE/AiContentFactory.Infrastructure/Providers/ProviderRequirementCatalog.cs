using AiContentFactory.Application.ContentFactory;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class ProviderRequirementCatalog : IProviderRequirementCatalog
{
    private readonly ProviderRequirement[] _requirements =
    [
        Text("Template", "Template", "", "No external key required for local template script generation."),
        Text("OpenAI", "OpenAI", "https://platform.openai.com/docs/api-reference", "Uses Authorization: Bearer API key. Keep billing limits enabled."),
        Text("Claude", "Claude", "https://docs.anthropic.com/en/api/getting-started", "Uses Anthropic API key and version header."),
        Text("Gemini", "Gemini", "https://ai.google.dev/gemini-api/docs/api-key", "Uses a Google AI Studio Gemini API key."),

        Video("Manual", "Manual", "", "No external video API. Agent creates planning artifacts only."),
        Video("Runway", "Runway", "https://docs.dev.runwayml.com/", "Requires a Runway API key."),
        Video("Pika", "Pika via fal.ai", "https://fal.ai/models/fal-ai/pika/api", "Pika is commonly consumed through fal.ai with a FAL API key."),
        Video("Luma", "Luma", "https://docs.lumalabs.ai/docs/video-generation", "Requires Luma API credentials for video generation."),

        Upload("DryRun", "Dry Run", "", "No external upload. Agent prepares upload checklist only."),
        Upload("YouTube", "YouTube Data API", "https://developers.google.com/youtube/v3/guides/authentication", "Uploads require OAuth client credentials and authorized tokens."),
        Upload("TikTok", "TikTok Content Posting API", "https://developers.tiktok.com/doc/content-posting-api-get-started", "Requires TikTok developer app credentials and access token."),
        Upload("Instagram", "Instagram Graph API", "https://developers.facebook.com/docs/instagram-platform/instagram-graph-api", "Requires Meta app credentials and publishing access token."),
        Upload("LinkedIn", "LinkedIn API", "https://learn.microsoft.com/en-us/linkedin/shared/authentication/authentication", "Requires OAuth client credentials and posting token."),

        Storage("LocalJson", "Local JSON", "", "Stores data on this machine under the API data folder."),
        Storage("OneDrive", "OneDrive", "https://learn.microsoft.com/en-us/graph/onedrive-concept-overview", "Requires Microsoft Graph app credentials and drive access."),
        Storage("S3", "Amazon S3", "https://docs.aws.amazon.com/AmazonS3/latest/userguide/security-creds.html", "Requires access key, secret key, region, and bucket."),
        Storage("GoogleDrive", "Google Drive", "https://developers.google.com/workspace/drive/api/guides/api-specific-auth", "Requires Google OAuth credentials and Drive scopes.")
    ];

    public IReadOnlyList<ProviderRequirement> List() => _requirements;

    private static ProviderRequirement Text(string name, string displayName, string docs, string notes)
    {
        var fields = name switch
        {
            "Template" => Array.Empty<ProviderCredentialField>(),
            "OpenAI" => [Secret("apiKey", "OpenAI API Key", "Paste key from OpenAI dashboard.")],
            "Claude" => [Secret("apiKey", "Anthropic API Key", "Paste key from Anthropic console."), TextField("version", "Anthropic Version", "Example: 2023-06-01")],
            "Gemini" => [Secret("apiKey", "Gemini API Key", "Paste key from Google AI Studio.")],
            _ => Array.Empty<ProviderCredentialField>()
        };

        return new ProviderRequirement("textProvider", name, displayName, docs, notes, fields);
    }

    private static ProviderRequirement Video(string name, string displayName, string docs, string notes)
    {
        var fields = name switch
        {
            "Runway" => [Secret("apiKey", "Runway API Key", "Create an API key in your Runway workspace.")],
            "Pika" => [Secret("falKey", "FAL API Key", "Use the fal.ai key for hosted Pika model endpoints.")],
            "Luma" => [Secret("apiKey", "Luma API Key", "Paste your Luma API key.")],
            _ => Array.Empty<ProviderCredentialField>()
        };

        return new ProviderRequirement("videoProvider", name, displayName, docs, notes, fields);
    }

    private static ProviderRequirement Upload(string name, string displayName, string docs, string notes)
    {
        var fields = name switch
        {
            "YouTube" => [TextField("clientId", "OAuth Client ID", "Google Cloud OAuth client ID."), Secret("clientSecret", "OAuth Client Secret", "Google Cloud OAuth client secret."), TextField("refreshToken", "Refresh Token", "Authorized refresh token for uploads.")],
            "TikTok" => [TextField("clientKey", "Client Key", "TikTok developer app client key."), Secret("clientSecret", "Client Secret", "TikTok developer app client secret."), TextField("accessToken", "Access Token", "Authorized posting access token.")],
            "Instagram" => [TextField("appId", "Meta App ID", "Meta app ID."), Secret("appSecret", "Meta App Secret", "Meta app secret."), TextField("accessToken", "Access Token", "Page or user token with publishing permissions.")],
            "LinkedIn" => [TextField("clientId", "Client ID", "LinkedIn app client ID."), Secret("clientSecret", "Client Secret", "LinkedIn app client secret."), TextField("accessToken", "Access Token", "Authorized access token for posting.")],
            _ => Array.Empty<ProviderCredentialField>()
        };

        return new ProviderRequirement("uploadProvider", name, displayName, docs, notes, fields);
    }

    private static ProviderRequirement Storage(string name, string displayName, string docs, string notes)
    {
        var fields = name switch
        {
            "OneDrive" => [TextField("clientId", "Microsoft Client ID", "Azure app registration client ID."), Secret("clientSecret", "Microsoft Client Secret", "Azure app registration secret."), TextField("tenantId", "Tenant ID", "Azure tenant ID."), TextField("driveId", "Drive ID", "Target OneDrive or SharePoint drive ID.")],
            "S3" => [TextField("accessKeyId", "Access Key ID", "IAM access key ID."), Secret("secretAccessKey", "Secret Access Key", "IAM secret access key."), TextField("region", "Region", "Example: us-east-1."), TextField("bucket", "Bucket", "Target S3 bucket name.")],
            "GoogleDrive" => [TextField("clientId", "Google Client ID", "Google Cloud OAuth client ID."), Secret("clientSecret", "Google Client Secret", "Google OAuth client secret."), TextField("refreshToken", "Refresh Token", "Authorized refresh token for Drive access."), TextField("folderId", "Folder ID", "Target Drive folder ID.")],
            _ => Array.Empty<ProviderCredentialField>()
        };

        return new ProviderRequirement("storageProvider", name, displayName, docs, notes, fields);
    }

    private static ProviderCredentialField Secret(string key, string label, string helpText) => new(key, label, "password", true, helpText);

    private static ProviderCredentialField TextField(string key, string label, string helpText) => new(key, label, "text", true, helpText);
}
