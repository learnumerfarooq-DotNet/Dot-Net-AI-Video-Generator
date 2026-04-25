import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

@Component({
  selector: 'app-agents-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './agents-page.component.html',
  styleUrl: './agents-page.component.css'
})
export class AgentsPageComponent {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly videoSearch = signal('');
  private readonly selectedVideoId = signal<string | null>(null);

  protected readonly activeDraft = computed(() => {
    const agent = this.store.activeAgent();
    return agent ? this.store.settingsDraft(agent.key) : null;
  });

  protected readonly driveStructure = computed(() => {
    const draft = this.activeDraft();
    const fallbackFolder = this.store.activeAgentVideos()[0]?.storageFolder ?? '';
    return this.toDriveSegments(draft?.storageFolderId || fallbackFolder);
  });

  protected readonly driveDestination = computed(() => {
    const segments = this.driveStructure();
    return segments.length ? segments[segments.length - 1] : '';
  });

  protected readonly filteredVideos = computed(() => {
    const query = this.videoSearch().trim().toLowerCase();
    const videos = [...this.store.activeAgentVideos()].sort(
      (left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt)
    );

    if (!query) {
      return videos;
    }

    return videos.filter((video) =>
      [
        video.title,
        video.topic,
        video.format,
        video.stage,
        video.storageFolder,
        video.platforms.join(' ')
      ].some((value) => value.toLowerCase().includes(query))
    );
  });

  protected readonly selectedVideo = computed(() => {
    const videos = this.filteredVideos();
    const selectedId = this.selectedVideoId();
    return videos.find((video) => video.id === selectedId) ?? videos[0] ?? null;
  });

  constructor() {
    effect(() => {
      const videos = this.filteredVideos();
      const selectedId = this.selectedVideoId();

      if (!videos.length) {
        if (selectedId !== null) {
          this.selectedVideoId.set(null);
        }

        return;
      }

      if (!selectedId || !videos.some((video) => video.id === selectedId)) {
        this.selectedVideoId.set(videos[0].id);
      }
    });
  }

  protected updateVideoSearch(value: string) {
    this.videoSearch.set(value);
  }

  protected selectVideo(videoId: string) {
    this.selectedVideoId.set(videoId);
  }

  protected isSourceVideoAgent(agentKey: string | undefined | null): boolean {
    return !!agentKey && (agentKey === 'video-generation-agent' || agentKey.startsWith('shorts-agent'));
  }

  protected folderStageLabel(index: number, total: number): string {
    if (index === 0) {
      return 'Workspace root';
    }

    if (index === total - 1) {
      return 'Agent delivery folder';
    }

    return 'Nested folder';
  }

  private toDriveSegments(value: string): string[] {
    return value
      .split(/[/\\>\n]+/)
      .map((segment) => segment.trim())
      .filter(Boolean);
  }
}
