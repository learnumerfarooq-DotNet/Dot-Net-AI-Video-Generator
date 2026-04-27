using AiContentFactory.Application.ContentFactory;
using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Backlog;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiContentFactory.Infrastructure.Backlog;

/// <summary>
/// PostgreSQL-backed implementation of IBacklogRepository.
/// Maps domain BacklogItems to/from studio_videos table records.
/// </summary>
public sealed class DbBacklogRepository(StudioDbContext dbContext) : IBacklogRepository
{
    public async Task<IReadOnlyList<BacklogItem>> ListAsync(BacklogStatus? status, CancellationToken cancellationToken)
    {
        var query = dbContext.Videos.AsQueryable();

        if (status is not null)
        {
            var stageStr = MapStatusToStage(status.Value);
            query = query.Where(v => v.Stage == stageStr);
        }

        var rows = await query
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(ToBacklogItem).ToArray();
    }

    public async Task<BacklogItem> AddAsync(BacklogItem item, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        dbContext.Videos.Add(new StudioVideoEntity
        {
            Id = item.Id,
            Title = item.Topic,
            Topic = item.Topic,
            Format = item.Format,
            Stage = MapStatusToStage(item.Status),
            StorageFolder = $"Google Drive / Backlog / {item.Platform}",
            DriveFileId = null,
            SourceAgentKey = "script-agent",
            Platforms = [item.Platform],
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            PublishedAt = null
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<BacklogItem?> UpdateStatusAsync(Guid id, BacklogStatus status, CancellationToken cancellationToken)
    {
        var video = await dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (video is null) return null;

        video.Stage = MapStatusToStage(status);
        video.UpdatedAt = DateTimeOffset.UtcNow;

        if (status == BacklogStatus.Ready)
            video.Stage = "ReadyToUpload";

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToBacklogItem(video);
    }

    private static BacklogItem ToBacklogItem(StudioVideoEntity video)
    {
        return new BacklogItem(
            video.Id,
            video.Topic,
            video.Platforms.FirstOrDefault() ?? "youtube",
            video.Format,
            MapStageToStatus(video.Stage))
        {
            Artifacts = new(),
            CreatedAt = video.CreatedAt,
            UpdatedAt = video.UpdatedAt
        };
    }

    private static string MapStatusToStage(BacklogStatus status) => status switch
    {
        BacklogStatus.Backlog => "Backlog",
        BacklogStatus.Ready => "ReadyToUpload",
        BacklogStatus.Published => "Published",
        _ => "Backlog"
    };

    private static BacklogStatus MapStageToStatus(string stage) => stage switch
    {
        "ReadyToUpload" => BacklogStatus.Ready,
        "Published" => BacklogStatus.Published,
        _ => BacklogStatus.Backlog
    };
}
