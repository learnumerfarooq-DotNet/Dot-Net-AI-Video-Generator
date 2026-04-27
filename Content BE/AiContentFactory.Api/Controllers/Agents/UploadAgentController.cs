using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Agents;

[ApiController]
[Route("api/agents/upload")]
public class UploadAgentController : ControllerBase
{
    private readonly IUploadAgent _uploadAgent;

    public UploadAgentController(IUploadAgent uploadAgent)
    {
        _uploadAgent = uploadAgent;
    }

    [HttpPost("prepare/{jobId}")]
    public async Task<ActionResult<UploadPackage>> PrepareUpload(Guid jobId, CancellationToken ct)
    {
        var package = await _uploadAgent.PrepareUploadAsync(jobId, ct);
        return Ok(package);
    }

    [HttpPost("{packageId}/metadata")]
    public async Task<ActionResult<UploadPackage>> GenerateMetadata(Guid packageId, CancellationToken ct)
    {
        var package = await _uploadAgent.GenerateMetadataAsync(packageId, ct);
        return Ok(package);
    }

    [HttpPost("{packageId}/schedule/{slotId}")]
    public async Task<IActionResult> AssignToScheduleSlot(Guid packageId, Guid slotId, CancellationToken ct)
    {
        await _uploadAgent.AssignToScheduleSlotAsync(packageId, slotId, ct);
        return Ok();
    }

    [HttpPost("{packageId}/execute")]
    public async Task<IActionResult> ExecuteUpload(Guid packageId, CancellationToken ct)
    {
        await _uploadAgent.ExecuteUploadAsync(packageId, ct);
        return Ok();
    }

    [HttpPost("{packageId}/approve")]
    public async Task<IActionResult> ApprovePackage(Guid packageId, CancellationToken ct)
    {
        await _uploadAgent.ApprovePackageAsync(packageId, ct);
        return Ok();
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<UploadPackage>>> GetPendingPackages(CancellationToken ct)
    {
        var packages = await _uploadAgent.GetPendingPackagesAsync(ct);
        return Ok(packages);
    }

    [HttpGet("{packageId}")]
    public async Task<ActionResult<UploadPackage>> GetPackage(Guid packageId, CancellationToken ct)
    {
        var package = await _uploadAgent.GetPackageAsync(packageId, ct);
        if (package == null) return NotFound();
        return Ok(package);
    }
}
