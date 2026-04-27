namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class TrendAgentLocalMemory
{
    public List<string> Top50Sites { get; set; } = new();
    public List<string> LastTrends { get; set; } = new();
    public List<string> TopKeywords { get; set; } = new();
    public List<string> ScheduleSlots { get; set; } = new();
    public List<int> PeakHours { get; set; } = new() { 8, 12, 18, 21 };
    public string OutputFolder { get; set; } = "/Scheduler/slots/";
    public DateTimeOffset? LastScrapeAt { get; set; }
    public double ScrapeSuccessRate { get; set; }
    public List<string> PreferredNiches { get; set; } = new();
    public List<string> AvoidedTopics { get; set; } = new();
    public bool FallbackToOpenRouter { get; set; } = true;
}
