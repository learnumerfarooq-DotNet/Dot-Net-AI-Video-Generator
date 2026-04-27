using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Backlog;

public sealed class BacklogItem : AggregateRoot
{
    public string Topic { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public BacklogStatus Status { get; set; }
    public List<ContentArtifact> Artifacts { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public BacklogItem() { }

    public BacklogItem(Guid id, string topic, string platform, string format, BacklogStatus status) : base(id)
    {
        Topic = topic;
        Platform = platform;
        Format = format;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
