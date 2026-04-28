using AiContentFactory.Application;
using AiContentFactory.Application.Common;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure;
using AiContentFactory.Infrastructure.Persistence;
using AiContentFactory.Infrastructure.Security;
using AiContentFactory.Api.Hubs;
using AiContentFactory.Infrastructure.Trends;
using AiContentFactory.Infrastructure.Analytics;
using AiContentFactory.Infrastructure.Brain;
using Hangfire;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Clear providers to avoid EventLog crash in some environments
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ── Core services ──────────────────────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddQuartz(q => {
    q.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService(opt => {
    opt.WaitForJobsToComplete = true;
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:4210")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<StudioDbContext>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IRealtimeEventEmitter, SignalREventEmitter>();

// ── App pipeline ───────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();



Console.WriteLine(">>> API Starting up...");

    // Seed / migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
    var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
    await StudioDatabaseInitializer.InitializeAsync(db, encryption, app.Lifetime.ApplicationStopping);

    // Initialize Local Memory
    var localMemory = scope.ServiceProvider.GetRequiredService<AiContentFactory.Application.Memory.ILocalMemoryService>();
    await localMemory.InitializeAllAgentMemoriesAsync(app.Lifetime.ApplicationStopping);

    // Schedule Recurring Trend Job
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    
    // Schedule Main Brain Orchestrator
    recurringJobManager.AddOrUpdate<MainBrainJob>(
        "main-brain-tick",
        job => job.ExecuteAsync(),
        "*/30 * * * * *"); // Every 30 seconds

    recurringJobManager.AddOrUpdate<TrendDiscoveryJob>(
        "hourly-trend-discovery",
        job => job.ExecuteAsync(),
        Cron.Hourly);

    recurringJobManager.AddOrUpdate<DailyAnalyticsJob>(
        "daily-analytics-loop",
        job => job.ExecuteAsync(),
        Cron.Daily(3));
}

app.UseHangfireDashboard();

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
app.MapHub<StudioHub>("/hubs/studio");

app.Run();
