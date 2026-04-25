using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Backlog;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Application.ContentFactory;

public sealed record CreateContentTaskRequest(
    string Topic,
    string Platform,
    string Format,
    string? Audience,
    string? Goal,
    bool AutoSaveLocalMemory = true);

public sealed record BrainRunResponse(
    Guid Id,
    string Topic,
    IReadOnlyList<string> Plan,
    IReadOnlyList<AgentRunResult> AgentResults,
    IReadOnlyList<MemoryEntry> MemoriesUsed,
    IReadOnlyList<MemorySuggestion> MemorySuggestions,
    DateTimeOffset CreatedAt);

public sealed record AgentRunResult(
    string AgentName,
    AgentRunStatus Status,
    string Summary,
    IReadOnlyList<ContentArtifact> Artifacts);

public sealed record MemorySearchRequest(MemoryScope? Scope, string? AgentName);

public sealed record ApproveMemorySuggestionRequest(string? RevisedContent);

public sealed record ProviderConfig(
    string TextProvider,
    string VideoProvider,
    string UploadProvider,
    string StorageProvider);

public sealed record ProviderCredentialField(
    string Key,
    string Label,
    string InputType,
    bool Required,
    string HelpText);

public sealed record ProviderRequirement(
    string ProviderType,
    string ProviderName,
    string DisplayName,
    string DocumentationUrl,
    string Notes,
    IReadOnlyList<ProviderCredentialField> Fields);

public sealed record ProviderCredentialStatus(
    string ProviderType,
    string ProviderName,
    IReadOnlyDictionary<string, bool> HasValues,
    DateTimeOffset? UpdatedAt);

public sealed record ProviderCredentialInput(
    string ProviderType,
    string ProviderName,
    IReadOnlyDictionary<string, string> Values);

public sealed record SaveProviderCredentialsRequest(
    ProviderConfig Config,
    IReadOnlyList<ProviderCredentialInput> Credentials);

public sealed record SavedProviderState(
    ProviderConfig Config,
    IReadOnlyList<ProviderCredentialStatus> CredentialStatus);
