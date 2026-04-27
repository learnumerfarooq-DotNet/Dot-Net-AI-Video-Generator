namespace AiContentFactory.Domain.GlobalMemory;

public enum SlotStatus
{
    Open = 0,
    Assigned = 1,
    Completed = 2,
    Missed = 3
}

public sealed class ScheduleSlot
{
    public Guid Id { get; set; }
    public DateTimeOffset SlotTime { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Guid? AssignedJobId { get; set; }
    public SlotStatus Status { get; set; }
    public List<string> Keywords { get; set; } = new();
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
