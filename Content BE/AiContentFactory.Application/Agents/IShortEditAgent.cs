using AiContentFactory.Domain.Agents;

namespace AiContentFactory.Application.Agents;

public interface IShortEditAgent
{
    Task<ShortEditPlan> CreateEditPlanAsync(Guid shortClipId, CancellationToken ct = default);
    Task ExecuteEditPlanAsync(Guid jobId, ShortEditPlan plan, CancellationToken ct = default);
    Task ExecuteAsync(Guid jobId, CancellationToken ct = default);
    Task<ShortEditPlan?> GetEditPlanAsync(Guid shortClipId, CancellationToken ct = default);
    Task ReExecuteAsync(Guid shortClipId, CancellationToken ct = default);
    Task<bool> ValidatePlanAsync(ShortEditPlan plan, CancellationToken ct = default);
}
