namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class ScriptGenLocalMemory
{
    public string LastScriptStyle { get; set; } = "educational";
    public string ToneConfig { get; set; } = "professional";
    public string VideoType { get; set; } = "long";
    public string PreferredLanguage { get; set; } = "en";
    public string OutputFolder { get; set; } = "/RAW/scripts/";
    public int MaxScriptLength { get; set; } = 2000;
    public bool IncludeCallToAction { get; set; } = true;
    public string HookStylePreference { get; set; } = "question";
    public List<string> KeywordFocusAreas { get; set; } = new();
    public string? LastGeneratedScript { get; set; }
}
