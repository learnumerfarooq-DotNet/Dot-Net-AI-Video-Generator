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

app.Run();
