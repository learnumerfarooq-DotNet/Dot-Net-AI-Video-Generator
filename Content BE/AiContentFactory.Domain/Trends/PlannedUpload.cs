namespace AiContentFactory.Domain.Trends;

public record PlannedUpload(
    string Topic,
    DateTimeOffset ScheduledTime,
    List<string> Platforms,
    List<string> Keywords,
    List<string> Hashtags,
    string Rationale,
    Guid? VideoId = null);
