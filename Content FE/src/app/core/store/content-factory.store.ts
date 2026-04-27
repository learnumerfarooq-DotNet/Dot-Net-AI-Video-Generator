import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import {
  MENU_BY_SECTION,
  SideNavItem,
  SideTabId,
  TOP_SECTION_ITEMS,
  ThemeMode,
  TopSectionId,
  WorkspaceBootstrap
} from '../models/content-factory.models';
import { WorkspaceService } from '../services/workspace.service';

// Feature Stores
import { DashboardStore } from '../../features/dashboard/store/dashboard.store';
import { AgentsStore } from '../../features/agents/store/agents.store';
import { MemoryStore } from '../../features/memory/store/memory.store';
import { DriveStore } from '../../features/drive/store/drive.store';
import { SettingsStore } from '../../features/settings/store/settings.store';
import { SchedulerStore } from '../../features/scheduler/store/scheduler.store';
import { PipelineStore } from './pipeline.store';
import { AnalyticsStore } from './analytics.store';
import { ErrorStore } from './error.store';

type StudioState = {
  activeSection: TopSectionId;
  activeSideTab: SideTabId;
  theme: ThemeMode;
  sidebarCollapsed: boolean;
  loading: boolean;
  status: string;
  workspace: WorkspaceBootstrap | null;
};

const initialState: StudioState = {
  activeSection: 'dashboard',
  activeSideTab: 'dashboard-overview',
  theme: 'light',
  sidebarCollapsed: false,
  loading: false,
  status: 'Ready',
  workspace: null
};

export const ContentFactoryStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => {
    const workspace = computed(() => store.workspace());

    return {
      sections: computed(() => TOP_SECTION_ITEMS),
      sideTabs: computed<SideNavItem[]>(() =>
        MENU_BY_SECTION[store.activeSection()].map((item) => ({
          ...item,
          badgeValue: getBadgeValue(item.badge, item.agentKey, workspace()) ?? ''
        }))
      ),
      currentSectionLabel: computed(() => TOP_SECTION_ITEMS.find((item) => item.id === store.activeSection())?.label ?? 'Dashboard'),
      currentSectionDescription: computed(() => TOP_SECTION_ITEMS.find((item) => item.id === store.activeSection())?.description ?? ''),
      currentSideTabLabel: computed(() => MENU_BY_SECTION[store.activeSection()].find((item) => item.id === store.activeSideTab())?.label ?? ''),
      activeAgentKey: computed(() => getActiveAgentKey(store.activeSection(), store.activeSideTab())),
      pendingMemoryCount: computed(() => workspace()?.memory.reviewQueue.length ?? 0),
      connectedAgentCount: computed(() => workspace()?.agents.agents.filter((agent) => agent.isConnected).length ?? 0),
      readyItems: computed(() => workspace()?.dashboard.readyVideos.length ?? 0),
      agents: computed(() => workspace()?.agents.agents ?? [])
    };
  }),
  withMethods((
    store,
    workspaceSvc = inject(WorkspaceService),
    dashboardStore = inject(DashboardStore),
    agentsStore = inject(AgentsStore),
    memoryStore = inject(MemoryStore),
    driveStore = inject(DriveStore),
    settingsStore = inject(SettingsStore),
    schedulerStore = inject(SchedulerStore),
    pipelineStore = inject(PipelineStore),
    analyticsStore = inject(AnalyticsStore),
    errorStore = inject(ErrorStore)
  ) => {
    async function refreshAll() {
      patchState(store, { loading: true, status: 'Syncing workspace...' });

      try {
        const workspace = await firstValueFrom(workspaceSvc.getBootstrap());
        
        // Hydrate the Master Store
        patchState(store, {
          workspace,
          loading: false,
          status: `Workspace synced at ${new Date(workspace.generatedAt).toLocaleTimeString()}.`
        });

        // Hydrate all Feature Stores
        dashboardStore.hydrate(workspace);
        agentsStore.hydrate(workspace);
        memoryStore.hydrate(workspace);
        driveStore.hydrate(workspace);
        settingsStore.hydrate(workspace);
        schedulerStore.hydrate(workspace);
        
        // NEW: Hydrate automation stores
        // (Assuming these are part of the bootstrap or added as extensions)
        // For now we hydrate with empty lists if missing in bootstrap
        pipelineStore.hydrate([]); 
        analyticsStore.hydrate([], []);

      } catch (error) {
        patchState(store, {
          loading: false,
          status: `Workspace sync failed: ${readError(error)}`
        });
      }
    }

    return {
      async init() {
        const savedTheme = window.localStorage.getItem('contentFactoryTheme');
        const savedSidebar = window.localStorage.getItem('contentFactorySidebar');

        patchState(store, {
          theme: savedTheme === 'dark' ? 'dark' : 'light',
          sidebarCollapsed: savedSidebar === 'collapsed'
        });

        await refreshAll();
      },

      refreshAll,

      setSection(section: TopSectionId) {
        let activeSideTab = MENU_BY_SECTION[section][0].id;
        
        patchState(store, {
          activeSection: section,
          activeSideTab: activeSideTab
        });
        agentsStore.setActiveAgentKey(getActiveAgentKey(section, activeSideTab));
      },

      setSideTab(tab: SideTabId) {
        patchState(store, { activeSideTab: tab });
        agentsStore.setActiveAgentKey(getActiveAgentKey(store.activeSection(), tab));
      },

      setTheme(theme: ThemeMode) {
        window.localStorage.setItem('contentFactoryTheme', theme);
        patchState(store, { theme });
      },

      toggleSidebar() {
        const next = !store.sidebarCollapsed();
        window.localStorage.setItem('contentFactorySidebar', next ? 'collapsed' : 'open');
        patchState(store, { sidebarCollapsed: next });
      },

      setActiveAgent(key: string) {
        agentsStore.setActiveAgentKey(key);
      },

      setStatus(status: string) {
        patchState(store, { status });
      }
    };
  })
);

function getActiveAgentKey(section: TopSectionId, tab: SideTabId): string | null {
  const item = MENU_BY_SECTION[section].find((i) => i.id === tab);
  return item?.agentKey ?? null;
}

function getBadgeValue(badgeType: SideNavItem['badge'], agentKey: string | undefined, workspace: WorkspaceBootstrap | null): string | undefined {
  if (!workspace || !badgeType) return undefined;

  switch (badgeType) {
    case 'memory-count':
      return workspace.memory.reviewQueue.length > 0 ? String(workspace.memory.reviewQueue.length) : undefined;
    case 'agent-status':
      const agent = workspace.agents.agents.find((a) => a.key === agentKey);
      return agent?.status === 'Running' ? 'Live' : undefined;
    case 'ready-count':
      return workspace.dashboard.readyVideos.length > 0 ? String(workspace.dashboard.readyVideos.length) : undefined;
    case 'job-count':
      const totalJobs = workspace.scheduler.manualSchedules.length + workspace.scheduler.dailyPostingJobs.length;
      return totalJobs > 0 ? String(totalJobs) : undefined;
    default:
      return undefined;
  }
}

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
