using AiContentFactory.Application;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure;
using AiContentFactory.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Core services ──────────────────────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddControllers();          // MVC controllers in feature folders

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<StudioDbContext>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── App pipeline ───────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();

// Seed / migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
    await StudioDatabaseInitializer.InitializeAsync(db, app.Lifetime.ApplicationStopping);
}

// Root info
app.MapGet("/", () => Results.Ok(new
{
    name         = "AI Content Factory API",
    architecture = "Angular + .NET + PostgreSQL studio workspace",
    status       = "ready"
}));

app.MapHealthChecks("/health");

// Feature controllers are discovered automatically via AddControllers()
// Brain/     → WorkspaceController   GET  /api/workspace/bootstrap
// Agents/    → AgentsController      POST /api/agents/{key}/chat
//                                    DEL  /api/agents/{key}/chat/cleanup
// Memory/    → MemoryController      POST /api/memory/{id}/approve|reject
//                                    GET  /api/memory/suggestions/pending
// Backlog/   → BacklogController     POST /api/videos/{id}/stage
// Providers/ → SchedulerController   POST /api/scheduler/manual
//             SettingsController     PUT  /api/settings/agents/{key}
// Storage/   → DriveController       *    /api/drive/**
app.MapControllers();

app.Run();
