namespace AiContentFactory.Application.Configuration;

public sealed class ContentFactoryOptions
{
    public const string SectionName = "ContentFactory";

    public string TextProvider { get; init; } = "Template";

    public string VideoProvider { get; init; } = "Manual";

    public string UploadProvider { get; init; } = "DryRun";

    public string StorageProvider { get; init; } = "LocalJson";

    public string DataPath { get; init; } = "data";
}
