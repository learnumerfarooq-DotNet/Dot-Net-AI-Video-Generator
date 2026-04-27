namespace AiContentFactory.Domain.Trends;

public record TrendingTopic(
    string Keyword,
    string Source,
    int Rank,
    double RelevanceScore,
    string Category);
