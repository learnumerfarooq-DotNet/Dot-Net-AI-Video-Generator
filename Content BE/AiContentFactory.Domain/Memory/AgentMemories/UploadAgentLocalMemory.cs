namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class UploadAgentLocalMemory
{
    public Dictionary<string, string> PlatformTokens { get; set; } = new();
    public Dictionary<string, string> AccountIds { get; set; } = new();
    public string TitleTemplate { get; set; } = "{topic} | {keyword} #shorts";
    public string DescriptionTemplate { get; set; } = "...";
    public List<string> HashtagBank { get; set; } = new();
    public string DefaultPrivacy { get; set; } = "public";
    public string DefaultCategory { get; set; } = "22";
    public string InputFolder { get; set; } = "/ReadyToUpload/";
    public List<Guid> LastUploadedIds { get; set; } = new();
    public List<string> PreferredPlatforms { get; set; } = new() { "YouTube", "TikTok", "Instagram" };
    public bool AutoSchedule { get; set; } = true;
    public int MaxUploadsPerDay { get; set; } = 10;
}
