using AiContentFactory.Application.Memory;
using AiContentFactory.Domain.GlobalMemory;
using Microsoft.AspNetCore.Mvc;

namespace AiContentFactory.Api.Controllers.Memory;

[ApiController]
[Route("api/memory/global")]
public class GlobalMemoryController : ControllerBase
{
    private readonly IGlobalMemoryService _globalMemoryService;

    public GlobalMemoryController(IGlobalMemoryService globalMemoryService)
    {
        _globalMemoryService = globalMemoryService;
    }

    [HttpGet]
    public async Task<ActionResult<GlobalMemory>> GetGlobalMemory(CancellationToken ct)
    {
        var memory = await _globalMemoryService.LoadAsync(ct);
        return Ok(memory);
    }

    [HttpPut]
    public async Task<IActionResult> SaveGlobalMemory([FromBody] GlobalMemory memory, CancellationToken ct)
    {
        await _globalMemoryService.SaveAsync(memory, ct);
        return Ok();
    }

    [HttpGet("folders")]
    public async Task<ActionResult<FolderRegistry>> GetFolderRegistry(CancellationToken ct)
    {
        var folders = await _globalMemoryService.GetFolderRegistryAsync(ct);
        return Ok(folders);
    }

    [HttpGet("constraints")]
    public async Task<ActionResult<VideoConstraints>> GetVideoConstraints(CancellationToken ct)
    {
        var constraints = await _globalMemoryService.GetVideoConstraintsAsync(ct);
        return Ok(constraints);
    }

    [HttpGet("trends-config")]
    public async Task<ActionResult<TrendAgentConfig>> GetTrendConfig(CancellationToken ct)
    {
        var config = await _globalMemoryService.GetTrendConfigAsync(ct);
        return Ok(config);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<GlobalMemory>> RefreshGlobalMemory(CancellationToken ct)
    {
        var memory = await _globalMemoryService.ForceRefreshAsync(ct);
        return Ok(memory);
    }
}
