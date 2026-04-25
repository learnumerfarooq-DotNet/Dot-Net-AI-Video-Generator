using AiContentFactory.Application;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure;
using AiContentFactory.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StudioDbContext>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
    await StudioDatabaseInitializer.InitializeAsync(dbContext, app.Lifetime.ApplicationStopping);
}

app.MapGet("/", () => Results.Ok(new
{
    name = "AI Content Factory API",
    architecture = "Angular + .NET + PostgreSQL studio workspace",
    status = "ready"
}));

app.MapHealthChecks("/health");

app.MapGet("/api/workspace/bootstrap", async (
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var response = await facade.GetBootstrapAsync(cancellationToken);
    return Results.Ok(response);
});

app.MapPost("/api/agents/{agentKey}/chat", async (
    string agentKey,
    SendAgentMessageRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var response = await facade.SendAgentMessageAsync(agentKey, request, cancellationToken);
    return Results.Ok(response);
});

app.MapPost("/api/memory/{id:guid}/approve", async (
    Guid id,
    ReviewMemoryRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var memory = await facade.ApproveMemoryAsync(id, request, cancellationToken);
    return memory is null ? Results.NotFound() : Results.Ok(memory);
});

app.MapPost("/api/memory/{id:guid}/reject", async (
    Guid id,
    ReviewMemoryRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var memory = await facade.RejectMemoryAsync(id, request, cancellationToken);
    return memory is null ? Results.NotFound() : Results.Ok(memory);
});

app.MapPost("/api/videos/{id:guid}/stage", async (
    Guid id,
    UpdateVideoStageRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var video = await facade.UpdateVideoStageAsync(id, request, cancellationToken);
    return video is null ? Results.NotFound() : Results.Ok(video);
});

app.MapPost("/api/scheduler/manual", async (
    CreateManualScheduleRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var schedule = await facade.CreateManualScheduleAsync(request, cancellationToken);
    return Results.Ok(schedule);
});

app.MapPut("/api/settings/agents/{agentKey}", async (
    string agentKey,
    SaveAgentSettingsRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var settings = await facade.SaveAgentSettingsAsync(agentKey, request, cancellationToken);
    return settings is null ? Results.NotFound() : Results.Ok(settings);
});

app.MapGet("/api/memory/suggestions/pending", async (
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var suggestions = await facade.GetPendingMemorySuggestionsAsync(cancellationToken);
    return Results.Ok(suggestions);
});
app.MapDelete("/api/agents/{agentKey}/chat/cleanup", async (
    string agentKey,
    AiContentFactory.Infrastructure.Persistence.StudioDbContext db,
    CancellationToken cancellationToken) =>
{
    var broken = db.ChatMessages
        .Where(m => m.AgentKey == agentKey && (
            m.Content.StartsWith("No response content generated") ||
            m.Content.StartsWith("Main Brain response")));
    db.ChatMessages.RemoveRange(broken);
    var count = await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { deleted = count, agentKey });
});

// Google Drive OAuth token exchange
app.MapPost("/api/drive/oauth/exchange", async (DriveOAuthExchangeRequest request, IConfiguration config) =>
{
    using var httpClient = new HttpClient();

    var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["code"] = request.Code,
        ["client_id"] = "",
        ["client_secret"] = "",
        ["redirect_uri"] = request.RedirectUri,
        ["grant_type"] = "authorization_code"
    });

    var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestBody);
    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(
            detail: responseBody,
            statusCode: (int)response.StatusCode,
            title: "Google OAuth token exchange failed");
    }

    var tokenDoc = System.Text.Json.JsonDocument.Parse(responseBody);
    var root = tokenDoc.RootElement;

    var accessToken = root.GetProperty("access_token").GetString() ?? "";
    var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;
    var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? "" : "";

    return Results.Ok(new
    {
        accessToken,
        refreshToken,
        expiresIn
    });
});

app.MapGet("/api/drive/config", async (
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    // ...
    return Results.Ok(); // Placeholder if needed, but bootstrap is better
});

app.MapGet("/api/drive/files", async (
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var files = await facade.ListDriveFilesAsync(cancellationToken);
    return Results.Ok(files);
});

app.MapPost("/api/drive/folders", async (
    CreateFolderRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var folder = await facade.CreateDriveFolderAsync(request.Name, cancellationToken);
    return folder is null ? Results.Problem("Failed to create folder") : Results.Ok(folder);
});

app.MapPut("/api/drive/config", async (
    SaveDriveSettingsRequest request,
    IStudioWorkspaceFacade facade,
    CancellationToken cancellationToken) =>
{
    var settings = await facade.SaveDriveSettingsAsync(request, cancellationToken);
    return Results.Ok(settings);
});

app.Run();

public record DriveOAuthExchangeRequest(string Code, string RedirectUri);
public record CreateFolderRequest(string Name);
