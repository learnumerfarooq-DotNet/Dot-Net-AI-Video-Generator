using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Backlog;

namespace AiContentFactory.Infrastructure.Providers;

public sealed class DryRunUploadExecutionProvider : IUploadExecutionProvider
{
    public Task<string> UploadAsync(BacklogItem item, CancellationToken cancellationToken)
        => Task.FromResult($"Dry run upload queued for {item.Platform}: {item.Topic}");
}
