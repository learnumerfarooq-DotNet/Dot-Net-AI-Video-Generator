namespace AiContentFactory.Application.Brain;

public sealed class BrainOptions
{
    public const string SectionName = "Brain";

    public int TickIntervalSeconds { get; set; } = 30;          // How often brain ticks
    public int GlobalMemorySyncIntervalSeconds { get; set; } = 60;  // How often to re-read global.json
    public int MaxConcurrentDispatches { get; set; } = 4;       // Max parallel agent dispatches
    public int MaxRetryPerJob { get; set; } = 3;                // Max retries before dead-letter
    public int CircuitBreakerThreshold { get; set; } = 5;       // Failures before circuit opens
    public int CircuitBreakerPauseMinutes { get; set; } = 10;   // How long circuit stays open
    public bool AutoDispatchOnRawDetected { get; set; } = true; // Auto-start pipeline on new RAW
    public bool EmitSignalREvents { get; set; } = true;         // Push events to Angular
    public string GlobalMemoryDrivePath { get; set; } = "/memory/global.json";
    public string[] PeakUploadSlotsUtc { get; set; } = { "08:00", "12:00", "18:00", "21:00" };
}
