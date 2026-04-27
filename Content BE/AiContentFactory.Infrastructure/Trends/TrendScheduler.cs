using AiContentFactory.Domain.Trends;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Trends;

public sealed class TrendScheduler
{
    private readonly TrendOptions _options;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<TrendScheduler> _logger;

    public TrendScheduler(
        IOptions<TrendOptions> options,
        IBackgroundJobClient jobClient,
        ILogger<TrendScheduler> logger)
    {
        _options = options.Value;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async Task ScheduleUploadsAsync(TrendDecisionPayload decision, CancellationToken ct = default)
    {
        _logger.LogInformation("Scheduling {Count} uploads based on trend analysis.", decision.PlannedUploads.Count);

        foreach (var upload in decision.PlannedUploads)
        {
            var scheduledTime = OptimizeToPeakSlot(upload.ScheduledTime);
            
            _logger.LogInformation("Scheduling upload for topic '{Topic}' at {Time}", upload.Topic, scheduledTime);

            // In a real implementation, we would store this in DB and queue a Hangfire job
            // For now, we simulate the queuing
            _jobClient.Schedule(() => _logger.LogInformation("Executing scheduled upload for {Topic}", upload.Topic), 
                scheduledTime);
        }

        await Task.CompletedTask;
    }

    public async Task<List<PlannedUpload>> ScheduleTrendsAsync(TrendResult trends, CancellationToken ct = default)
    {
        _logger.LogInformation("Scheduling {Count} topics from TrendResult.", trends.Topics.Count);
        
        var planned = new List<PlannedUpload>();
        var now = DateTimeOffset.UtcNow;

        foreach (var topic in trends.Topics.Take(5))
        {
            var scheduledTime = OptimizeToPeakSlot(now.AddHours(topic.Rank * 2));
            planned.Add(new PlannedUpload(
                topic.Keyword,
                scheduledTime,
                topic.SuggestedPlatforms,
                new List<string> { topic.Keyword },
                new List<string> { "#" + topic.Keyword.Replace(" ", "") },
                topic.Rationale
            ));
        }

        return await Task.FromResult(planned);
    }

    private DateTimeOffset OptimizeToPeakSlot(DateTimeOffset requestedTime)
    {
        // Simple logic: find the closest peak slot defined in options
        // PeakSlotsUtc: ["08:00", "12:00", "18:00", "21:00"]
        
        if (!_options.PeakSlotsUtc.Any()) return requestedTime;

        var slots = _options.PeakSlotsUtc
            .Select(s => TimeSpan.Parse(s))
            .OrderBy(t => t)
            .ToList();

        var timeOfDay = requestedTime.TimeOfDay;
        var bestSlot = slots.Where(s => s >= timeOfDay).Cast<TimeSpan?>().FirstOrDefault() ?? slots.First();

        return new DateTimeOffset(requestedTime.Date.Add(bestSlot), requestedTime.Offset);
    }
}
