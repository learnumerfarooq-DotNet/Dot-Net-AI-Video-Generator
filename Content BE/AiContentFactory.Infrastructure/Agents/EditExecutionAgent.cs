using AiContentFactory.Application.Common;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Events;
using AiContentFactory.Domain.Memory.AgentMemories;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Processing;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class EditExecutionAgent : IEditAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly IFFmpegService _ffmpeg;
    private readonly ILocalMemoryService _localMemory;
    private readonly IGoogleDriveService _drive;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ITempStorageManager _tempStorage;
    private readonly IRealtimeEventEmitter _emitter;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<EditExecutionAgent> _logger;

    public EditExecutionAgent(
        IDecisionEngine decisionEngine,
        IFFmpegService ffmpeg,
        ILocalMemoryService localMemory,
        IGoogleDriveService drive,
        IStudioWorkspaceStore workspaceStore,
        IPipelineOrchestrator orchestrator,
        ITempStorageManager tempStorage,
        IRealtimeEventEmitter emitter,
        StudioDbContext dbContext,
        ILogger<EditExecutionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _ffmpeg = ffmpeg;
        _localMemory = localMemory;
        _drive = drive;
        _workspaceStore = workspaceStore;
        _orchestrator = orchestrator;
        _tempStorage = tempStorage;
        _emitter = emitter;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EditPlan> CreateEditPlanAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _dbContext.VideoPipelineJobs.FindAsync(new object[] { jobId }, ct) ?? throw new Exception("Job not found");
        var script = await _dbContext.ScriptOutputs.FirstOrDefaultAsync(s => s.JobId == jobId, ct) ?? throw new Exception("Script not found");
        var localMemory = await _localMemory.GetConfigAsync<EditAgentLocalMemory>("edit-agent", ct) ?? new EditAgentLocalMemory();

        // Normally we'd download and analyze the file here
        // For simplicity, we just use a mocked context
        var context = new Dictionary<string, string>
        {
            { "scriptId", script.Id.ToString() },
            { "cutStyle", localMemory.CutStyle },
            { "transitionType", localMemory.TransitionType },
            { "captionTemplate", localMemory.CaptionTemplate }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("edit-agent", DecisionType.VideoEditing, context, jobId, ct);
        var payload = ParseEditDecision(decision);

        var plan = new EditPlan
        {
            JobId = jobId,
            ScriptId = script.Id,
            Segments = payload.Segments,
            Captions = payload.Captions,
            AudioAdjustments = payload.AudioAdjustments,
            Transitions = new List<TransitionPlan>(), // Simplified
            ColorGrading = payload.ColorGrading,
            Status = EditPlanStatus.Planned,
            InputDriveFileId = job.DriveFileId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.EditPlans.Add(plan);
        await _dbContext.SaveChangesAsync(ct);
        return plan;
    }

    public async Task ExecuteEditPlanAsync(Guid jobId, EditPlan plan, CancellationToken ct = default)
    {
        try
        {
            plan.Status = EditPlanStatus.Executing;
            await _dbContext.SaveChangesAsync(ct);

            // Fake processing with FFmpeg
            var tempDir = _tempStorage.CreateJobDirectory(jobId);
            var inputPath = Path.Combine(tempDir, "input.mp4"); // Assuming downloaded
            var outputPath = Path.Combine(tempDir, "output.mp4");

            // Mock an FFmpeg operation via builder
            var builder = new AiContentFactory.Infrastructure.Processing.FFmpegCommandBuilder()
                .Input(inputPath)
                .Output(outputPath)
                .OverwriteOutput();
            
            if (plan.ColorGrading != null)
                builder.ApplyColorGrading(plan.ColorGrading);

            // Execute mock logic - normally await _ffmpeg.ExecuteAsync(...)
            // We just sleep and mock success for this task.
            await Task.Delay(1000, ct);

            plan.Status = EditPlanStatus.Completed;
            await _dbContext.SaveChangesAsync(ct);

            await _localMemory.RecordRunAsync("edit-agent", true, null, ct);
            await _orchestrator.TransitionStageAsync(jobId, PipelineStageType.VideoEditing, ct);
            await _emitter.EmitVideoEditedAsync(new VideoEditedPayload(jobId, "drive-output-id", 60), ct);
        }
        catch (Exception ex)
        {
            plan.Status = EditPlanStatus.Failed;
            await _dbContext.SaveChangesAsync(ct);
            await _localMemory.RecordRunAsync("edit-agent", false, ex.Message, ct);
            throw;
        }
    }

    public async Task<EditPlan?> GetEditPlanAsync(Guid jobId, CancellationToken ct = default)
    {
        return await _dbContext.EditPlans.OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync(p => p.JobId == jobId, ct);
    }

    public async Task ReExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var plan = await GetEditPlanAsync(jobId, ct) ?? throw new Exception("No edit plan found.");
        await ExecuteEditPlanAsync(jobId, plan, ct);
    }

    public Task<VideoAnalysisResult> AnalyzeVideoAsync(string filePath, CancellationToken ct = default)
    {
        return Task.FromResult(new VideoAnalysisResult { Duration = 60.0 });
    }

    private EditDecisionPayload ParseEditDecision(AgentDecision decision)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<EditDecisionPayload>(decision.ValidatedPayload, opts) 
               ?? new EditDecisionPayload();
    }

    public async Task ExecuteAsync(Guid jobId, EditDecisionPayload payload, CancellationToken ct = default)
    {
        var plan = await CreateEditPlanAsync(jobId, ct);
        await ExecuteEditPlanAsync(jobId, plan, ct);
    }
}

public class EditDecisionPayload
{
    public List<EditSegment> Segments { get; set; } = new();
    public List<EditCaption> Captions { get; set; } = new();
    public List<AudioAdjustment> AudioAdjustments { get; set; } = new();
    public ColorGradingConfig? ColorGrading { get; set; }
}
