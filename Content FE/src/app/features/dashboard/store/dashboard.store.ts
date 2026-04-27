import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { WorkspaceService } from '../../../core/services/workspace.service';
import { DashboardService } from '../services/dashboard.service';
import {
  DashboardWorkspace,
  ReadyVideoListItem,
  VideoItem,
  WorkspaceBootstrap,
  LegacyVideoTaskDraft,
  LegacyVideoTaskRun,
  INITIAL_VIDEO_TASK
} from '../../../core/models/content-factory.models';

type DashboardState = {
  dashboard: DashboardWorkspace | null;
  agentRuns: any[]; // Paginated agent runs
  totalHistoryItems: number;
  currentHistoryPage: number;
  task: LegacyVideoTaskDraft;
  latestTaskRun: LegacyVideoTaskRun | null;
  loading: boolean;
  status: string;
};

const initialState: DashboardState = {
  dashboard: null,
  agentRuns: [],
  totalHistoryItems: 0,
  currentHistoryPage: 1,
  task: INITIAL_VIDEO_TASK,
  latestTaskRun: null,
  loading: false,
  status: 'Ready'
};

export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    readyVideoItems: computed<ReadyVideoListItem[]>(() => buildReadyVideoItems(store.dashboard())),
    readyCount:      computed(() => store.dashboard()?.readyVideos.length ?? 0),
    publishedCount:  computed(() => store.dashboard()?.recentlyPublished.length ?? 0),
    usageSeriesCount: computed(() => store.dashboard()?.usageSeries.length ?? 0),
    latestRun:       computed(() => store.latestTaskRun())
  })),
  withMethods((store,
    workspaceSvc = inject(WorkspaceService),
    dashboardSvc = inject(DashboardService)) => {
    
    async function loadHistory(page: number) {
      try {
        const result = await firstValueFrom(dashboardSvc.getAgentRuns(page));
        patchState(store, { 
          agentRuns: result.items, 
          totalHistoryItems: result.totalCount, 
          currentHistoryPage: page 
        });
      } catch (error) {
        console.error('Failed to load agent history:', error);
      }
    }

    return {
      hydrate(workspace: WorkspaceBootstrap) {
        patchState(store, {
          dashboard: workspace.dashboard,
          status: `Dashboard synced at ${new Date(workspace.generatedAt).toLocaleTimeString()}.`
        });
      },

      async loadSummary() {
        patchState(store, { loading: true });
        try {
          const summary = await firstValueFrom(dashboardSvc.getSummary());
          patchState(store, { dashboard: summary, loading: false });
          // Also load history on main load
          await loadHistory(1);
        } catch (error) {
          patchState(store, { loading: false, status: `Failed to load dashboard: ${readError(error)}` });
        }
      },

      loadHistory,

      async moveVideoStage(videoId: string, stage: string) {
        await firstValueFrom(dashboardSvc.updateVideoStage(videoId, stage));
      },

      updateTaskField<K extends keyof LegacyVideoTaskDraft>(key: K, value: LegacyVideoTaskDraft[K]) {
        patchState(store, {
          task: { ...store.task(), [key]: value }
        });
      },

      async runTask() {
        const task = store.task();
        const topic = task.topic.trim();
        if (!topic) {
          patchState(store, { status: 'Add a topic before running the video task.' });
          return;
        }

        patchState(store, { loading: true, status: 'Preparing video task...' });

        try {
          const audience = task.audience.trim() || 'general audience';
          const goal = task.goal.trim() || 'Generate the next publishable asset.';
          const latestTaskRun: LegacyVideoTaskRun = {
            topic,
            platform: task.platform,
            format: task.format,
            audience,
            goal,
            createdAt: new Date().toISOString(),
            agentResults: [
              { agentName: 'Trend Agent', summary: `Researched ideas and hooks for ${topic}.` },
              { agentName: 'Script Agent', summary: `Drafted a ${task.format} script for ${task.platform} aimed at ${audience}.` },
              { agentName: 'Main Brain', summary: goal }
            ]
          };

          patchState(store, {
            latestTaskRun,
            loading: false,
            status: 'Video task prepared in the dashboard workspace.'
          });
        } catch (error) {
          patchState(store, {
            loading: false,
            status: `Video task failed: ${readError(error)}`
          });
        }
      }
    };
  })
);

function buildReadyVideoItems(dashboard: DashboardWorkspace | null): ReadyVideoListItem[] {
  if (!dashboard) return [];
  return dashboard.readyVideos.flatMap((video: VideoItem) => {
    const platforms = video.platforms.length ? video.platforms : ['Unassigned'];
    return platforms.map((platform) => ({ id: `${video.id}:${platform}`, topic: video.topic, platform, format: video.format }));
  });
}

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
