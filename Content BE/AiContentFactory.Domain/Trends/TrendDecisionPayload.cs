namespace AiContentFactory.Domain.Trends;

public record TrendDecisionPayload(
    List<TrendingTopic> Topics,
    List<PlannedUpload> PlannedUploads,
    string AnalysisSummary,
    DateTimeOffset ValidUntil);
