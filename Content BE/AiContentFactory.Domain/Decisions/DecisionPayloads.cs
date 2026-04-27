namespace AiContentFactory.Domain.Decisions;

public record ScriptDecisionPayload(
    string Title,
    string Hook,
    string Body,
    string CallToAction,
    List<string> Keywords);

public record EditDecisionPayload(
    List<VideoSegment> Segments,
    List<CaptionOverlay> Captions,
    List<VisualEffect> Effects);

public record VideoSegment(
    double StartTime,
    double EndTime,
    string Description,
    string? Transition);

public record CaptionOverlay(
    double StartTime,
    double EndTime,
    string Text,
    string Style);

public record VisualEffect(
    double StartTime,
    string EffectType,
    string Parameters);

public record ShortDecisionPayload(
    string ParentVideoId,
    List<ShortSegment> Shorts);

public record ShortSegment(
    double StartTime,
    double EndTime,
    string Title,
    string Hook,
    string Rationale);

public record TrendDecisionPayload(
    List<TrendingTopic> Topics,
    List<PlannedUpload> PlannedUploads,
    string AnalysisSummary,
    DateTimeOffset ValidUntil);

public record TrendingTopic(
    string Keyword,
    string Source,
    double RelevanceScore);

public record PlannedUpload(
    string Topic,
    DateTimeOffset ScheduledTime,
    List<string> Platforms);

public record UploadDecisionPayload(
    string Title,
    string Description,
    List<string> Tags,
    string Category,
    bool IsPublic);
