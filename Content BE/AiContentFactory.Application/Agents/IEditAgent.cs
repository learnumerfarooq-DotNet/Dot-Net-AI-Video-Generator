using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;

namespace AiContentFactory.Application.Agents;

public interface IEditAgent
{
    Task<EditPlan> CreateEditPlanAsync(Guid jobId, CancellationToken ct = default);
    Task ExecuteEditPlanAsync(Guid jobId, EditPlan plan, CancellationToken ct = default);
    Task<EditPlan?> GetEditPlanAsync(Guid jobId, CancellationToken ct = default);
    Task ReExecuteAsync(Guid jobId, CancellationToken ct = default);
    Task<VideoAnalysisResult> AnalyzeVideoAsync(string filePath, CancellationToken ct = default);
}
