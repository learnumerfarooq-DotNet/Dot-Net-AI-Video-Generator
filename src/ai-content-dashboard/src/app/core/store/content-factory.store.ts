import { computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import {
  AgentChatResponse,
  AgentSettings,
  AgentSummary,
  INITIAL_MANUAL_SCHEDULE,
  INITIAL_VIDEO_TASK,
  LegacyVideoTaskDraft,
  LegacyVideoTaskRun,
  MENU_BY_SECTION,
  ManualScheduleDraft,
  MemoryRecord,
  PROVIDER_SELECT_OPTIONS,
  ProviderRequirement,
  ProviderRequirementField,
  ProviderSelections,
  ReadyVideoListItem,
  MemorySuggestionDto,
  SaveAgentSettingsRequest,
  SETTINGS_NAV_ITEMS,
  SideNavItem,
  SideTabId,
  TOP_SECTION_ITEMS,
  ThemeMode,
  TopSectionId,
  VideoItem,
  WorkspaceBootstrap
} from '../models/content-factory.models';

type StudioState = {
  activeSection: TopSectionId;
  activeSideTab: SideTabId;
  theme: ThemeMode;
  sidebarCollapsed: boolean;
  loading: boolean;
  status: string;
  workspace: WorkspaceBootstrap | null;
  chatDraft: string;
  sendingChat: boolean;
  memoryDrafts: Record<string, { title: string; content: string }>;
  manualSchedule: ManualScheduleDraft;
  creatingManualSchedule: boolean;
  settingsDrafts: Record<string, SaveAgentSettingsRequest>;
  savingAgentKey: string | null;
  providers: ProviderSelections;
  savingProviders: boolean;
  providerCredentials: Record<string, Record<string, string>>;
  task: LegacyVideoTaskDraft;
  latestTaskRun: LegacyVideoTaskRun | null;
  pendingMemorySuggestions: MemorySuggestionDto[];
  driveFiles: any[];
  driveConfig: any | null;
  loadingDrive: boolean;
};

const initialState: StudioState = {
  activeSection: 'dashboard',
  activeSideTab: 'dashboard-overview',
  theme: 'light',
  sidebarCollapsed: false,
  loading: false,
  status: 'Ready',
  workspace: null,
  chatDraft: '',
  sendingChat: false,
  memoryDrafts: {},
  manualSchedule: INITIAL_MANUAL_SCHEDULE,
  creatingManualSchedule: false,
  settingsDrafts: {},
  savingAgentKey: null,
  providers: emptyProviderSelections(),
  savingProviders: false,
  providerCredentials: {},
  task: INITIAL_VIDEO_TASK,
  latestTaskRun: null,
  pendingMemorySuggestions: [],
  driveFiles: [],
  driveConfig: null,
  loadingDrive: false
};

const apiBase = 'http://localhost:5039';

const PROVIDER_REQUIREMENTS: Record<string, ProviderRequirement> = {
  OpenAI: {
    providerName: 'OpenAI',
    displayName: 'OpenAI',
    providerType: 'Text / reasoning',
    notes: 'Use an API key and optional custom base URL for hosted or gateway traffic.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  Claude: {
    providerName: 'Claude',
    displayName: 'Claude',
    providerType: 'Text / writing',
    notes: 'Claude integrations normally need an API key and can share the same custom endpoint pattern.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  Gemini: {
    providerName: 'Gemini',
    displayName: 'Gemini',
    providerType: 'Text / multimodal',
    notes: 'Gemini can run with a direct API key or through a custom gateway endpoint.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  OpenRouter: {
    providerName: 'OpenRouter',
    displayName: 'OpenRouter',
    providerType: 'Model router',
    notes: 'OpenRouter adds a routing API key plus the model slug you want each agent to use.',
    fields: [
      { id: 'openRouterApiKey', label: 'OpenRouter API Key', inputType: 'password', helpText: 'Paste the OpenRouter API key.' },
      { id: 'openRouterModel', label: 'OpenRouter Model', inputType: 'text', helpText: 'Example: openai/gpt-4.1-mini.' }
    ]
  },
  Runway: {
    providerName: 'Runway',
    displayName: 'Runway',
    providerType: 'Video generation',
    notes: 'Runway video generation typically relies on an API key and optional endpoint override.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  Pika: {
    providerName: 'Pika',
    displayName: 'Pika',
    providerType: 'Video generation',
    notes: 'Pika connections usually use an API key plus optional custom endpoint settings.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  Luma: {
    providerName: 'Luma',
    displayName: 'Luma',
    providerType: 'Video generation',
    notes: 'Luma video flows can be staged with an API key and optional base URL.',
    fields: [
      { id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' },
      { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }
    ]
  },
  Manual: {
    providerName: 'Manual',
    displayName: 'Manual Handoff',
    providerType: 'No-code fallback',
    notes: 'Use this when the workflow is handled manually and no external credential is required.',
    fields: []
  },
  YouTube: {
    providerName: 'YouTube',
    displayName: 'YouTube Upload',
    providerType: 'Publishing',
    notes: 'OAuth credentials let publishing agents upload and manage YouTube content.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }
    ]
  },
  TikTok: {
    providerName: 'TikTok',
    displayName: 'TikTok Upload',
    providerType: 'Publishing',
    notes: 'TikTok publishing credentials are staged here for later per-agent save actions.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }
    ]
  },
  Instagram: {
    providerName: 'Instagram',
    displayName: 'Instagram Upload',
    providerType: 'Publishing',
    notes: 'Instagram and Meta publishing flows typically need OAuth credentials.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }
    ]
  },
  Facebook: {
    providerName: 'Facebook',
    displayName: 'Facebook Upload',
    providerType: 'Publishing',
    notes: 'Meta publishing credentials are staged here for later per-agent save actions.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }
    ]
  },
  LinkedIn: {
    providerName: 'LinkedIn',
    displayName: 'LinkedIn Upload',
    providerType: 'Publishing',
    notes: 'LinkedIn publishing connections usually rely on OAuth credentials.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }
    ]
  },
  DryRun: {
    providerName: 'DryRun',
    displayName: 'Dry Run',
    providerType: 'Publishing fallback',
    notes: 'Dry run mode skips external publishing and does not need credentials.',
    fields: []
  },
  'Google Drive': {
    providerName: 'Google Drive',
    displayName: 'Google Drive',
    providerType: 'Storage',
    notes: 'Drive credentials and folder metadata keep upload targets aligned across agents.',
    fields: [
      { id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' },
      { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' },
      { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' },
      { id: 'storageFolderId', label: 'Folder ID / Path', inputType: 'text', helpText: 'Drive folder ID or readable folder tree.' }
    ]
  },
  'Local Storage': {
    providerName: 'Local Storage',
    displayName: 'Local Storage',
    providerType: 'Storage',
    notes: 'Local storage uses filesystem paths only and does not require external credentials.',
    fields: [{ id: 'sourceVideoPath', label: 'Source Video Path', inputType: 'text', helpText: 'Reusable local folder path.' }]
  }
};

export const ContentFactoryStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => {
    const workspace = computed(() => store.workspace());
    const activeAgentKey = computed(() => getActiveAgentKey(store.activeSection(), store.activeSideTab()));
    const activeAgent = computed<AgentSummary | null>(() => {
      const currentWorkspace = workspace();
      const key = activeAgentKey();
      return key && currentWorkspace ? currentWorkspace.agents.agents.find((agent) => agent.key === key) ?? null : null;
    });
    const activeAgentSettings = computed<AgentSettings | null>(() => {
      const currentWorkspace = workspace();
      const key = activeAgentKey();
      return key && currentWorkspace ? currentWorkspace.settings.agents.find((agent) => agent.agentKey === key) ?? null : null;
    });
    const activeAgentVideos = computed<VideoItem[]>(() => {
      const currentWorkspace = workspace();
      const key = activeAgentKey();
      if (!currentWorkspace || !key) {
        return [];
      }

      return [...currentWorkspace.dashboard.backlogVideos, ...currentWorkspace.dashboard.readyVideos, ...currentWorkspace.dashboard.recentlyPublished]
        .filter((video) => video.sourceAgentKey === key);
    });
    const providerOptionsForActiveAgent = computed<string[]>(() => {
      const category = activeAgentSettings()?.category ?? activeAgent()?.category ?? '';
      const currentWorkspace = workspace();
      const fromWorkspace = currentWorkspace?.settings.providerOptions.find((option) => option.category === category)?.providers ?? [];
      return fromWorkspace.length ? fromWorkspace : fallbackProviderOptions(category);
    });
    const selectedRequirements = computed<ProviderRequirement[]>(() => buildSelectedRequirements(store.providers()));
    const readyVideoItems = computed<ReadyVideoListItem[]>(() => buildReadyVideoItems(workspace()));

    return {
      sections: computed(() => TOP_SECTION_ITEMS),
      sideTabs: computed<SideNavItem[]>(() =>
        MENU_BY_SECTION[store.activeSection()].map((item) => ({
          ...item,
          badgeValue: getBadgeValue(item.badge, item.agentKey, workspace())
        }))
      ),
      currentSectionLabel: computed(() => TOP_SECTION_ITEMS.find((item) => item.id === store.activeSection())?.label ?? 'Dashboard'),
      currentSectionDescription: computed(() => TOP_SECTION_ITEMS.find((item) => item.id === store.activeSection())?.description ?? ''),
      currentSideTabLabel: computed(() => MENU_BY_SECTION[store.activeSection()].find((item) => item.id === store.activeSideTab())?.label ?? ''),
      activeAgentKey,
      activeAgent,
      activeAgentSettings,
      activeAgentVideos,
      providerOptionsForActiveAgent,
      dashboard: computed(() => workspace()?.dashboard ?? null),
      memory: computed(() => workspace()?.memory ?? null),
      scheduler: computed(() => workspace()?.scheduler ?? null),
      activeAgentMessages: computed(() => {
        const currentWorkspace = workspace();
        const key = activeAgentKey();
        return key && currentWorkspace ? currentWorkspace.agents.chatMessages.filter((message) => message.agentKey === key) : [];
      }),
      pendingMemoryCount: computed(() => workspace()?.memory.reviewQueue.length ?? 0),
      connectedAgentCount: computed(() => workspace()?.agents.agents.filter((agent) => agent.isConnected).length ?? 0),
      selectedRequirements,
      pendingMemorySuggestions: computed<MemorySuggestionDto[]>(() => store.pendingMemorySuggestions()),
      latestRun: computed(() => store.latestTaskRun()),
      readyVideoItems,
      readyItems: computed(() => readyVideoItems().length),
      isDriveConfigured: computed(() => !!store.driveConfig()?.clientId && !!store.driveConfig()?.refreshToken)
    };
  }),
  withMethods((store, http = inject(HttpClient)) => ({
    async init() {
      const savedTheme = window.localStorage.getItem('contentFactoryTheme');
      const savedSidebar = window.localStorage.getItem('contentFactorySidebar');

      patchState(store, {
        theme: savedTheme === 'dark' ? 'dark' : 'light',
        sidebarCollapsed: savedSidebar === 'collapsed'
      });

      await this.refreshAll();
      await this.loadPendingMemorySuggestions();
    },

    async loadPendingMemorySuggestions() {
      try {
        const data = await firstValueFrom(http.get<MemorySuggestionDto[]>(`${apiBase}/api/memory/suggestions/pending`));
        patchState(store, { pendingMemorySuggestions: data ?? [] });
      } catch {
        // swallow errors in MVP
      }
    },

    setSection(section: TopSectionId) {
      let activeSideTab = MENU_BY_SECTION[section][0].id;
      
      // If drive is not configured, force config tab
      if (section === 'drive' && !store.isDriveConfigured()) {
        activeSideTab = 'drive-config';
      }

      patchState(store, {
        activeSection: section,
        activeSideTab: activeSideTab
      });
    },

    setSideTab(tab: SideTabId) {
      patchState(store, { activeSideTab: tab });
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

    updateChatDraft(value: string) {
      patchState(store, { chatDraft: value });
    },

    memoryDraft(memory: MemoryRecord) {
      return store.memoryDrafts()[memory.id] ?? { title: memory.title, content: memory.content };
    },

    updateMemoryDraft(memory: MemoryRecord, field: 'title' | 'content', value: string) {
      patchState(store, {
        memoryDrafts: {
          ...store.memoryDrafts(),
          [memory.id]: {
            ...this.memoryDraft(memory),
            [field]: value
          }
        }
      });
    },

    updateManualScheduleField<K extends keyof ManualScheduleDraft>(key: K, value: ManualScheduleDraft[K]) {
      patchState(store, {
        manualSchedule: {
          ...store.manualSchedule(),
          [key]: value
        }
      });
    },

    settingsDraft(agentKey: string): SaveAgentSettingsRequest {
      const existing = store.settingsDrafts()[agentKey];
      if (existing) {
        return existing;
      }

      const agent = store.workspace()?.settings.agents.find((item) => item.agentKey === agentKey);
      return agent ? toSettingsRequest(agent) : emptySettingsDraft();
    },

    updateSettingsDraft<K extends keyof SaveAgentSettingsRequest>(agentKey: string, key: K, value: SaveAgentSettingsRequest[K]) {
      const current = this.settingsDraft(agentKey);
      const next = mergeSettingsDraft(current, key, value);

      patchState(store, {
        settingsDrafts: {
          ...store.settingsDrafts(),
          [agentKey]: next
        }
      });
    },

    updateProviderField<K extends keyof ProviderSelections>(key: K, value: ProviderSelections[K]) {
      patchState(store, {
        providers: {
          ...store.providers(),
          [key]: value
        }
      });
    },

    credentialValue(requirement: ProviderRequirement, field: ProviderRequirementField): string {
      return store.providerCredentials()[requirement.providerName]?.[field.id] ?? '';
    },

    updateCredential(requirement: ProviderRequirement, field: ProviderRequirementField, value: string) {
      patchState(store, {
        providerCredentials: {
          ...store.providerCredentials(),
          [requirement.providerName]: {
            ...(store.providerCredentials()[requirement.providerName] ?? {}),
            [field.id]: value
          }
        }
      });
    },

    isCredentialSaved(requirement: ProviderRequirement, field: ProviderRequirementField): boolean {
      return this.credentialValue(requirement, field).trim().length > 0;
    },

    async saveProviders() {
      patchState(store, { savingProviders: true, status: 'Saving provider selections...' });

      try {
        patchState(store, {
          savingProviders: false,
          status: 'Provider selections staged locally. Use agent settings to persist per-agent credentials.'
        });
      } catch (error) {
        patchState(store, {
          savingProviders: false,
          status: `Provider save failed: ${readErrorMessage(error)}`
        });
      }
    },

    updateTaskField<K extends keyof LegacyVideoTaskDraft>(key: K, value: LegacyVideoTaskDraft[K]) {
      patchState(store, {
        task: {
          ...store.task(),
          [key]: value
        }
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
          status: `Video task failed: ${readErrorMessage(error)}`
        });
      }
    },

    async refreshAll() {
      patchState(store, { loading: true, status: 'Syncing workspace...' });

      try {
        const workspace = await firstValueFrom(http.get<WorkspaceBootstrap>(`${apiBase}/api/workspace/bootstrap`));
        patchState(store, {
          workspace,
          memoryDrafts: buildMemoryDrafts(workspace),
          settingsDrafts: buildSettingsDrafts(workspace),
          providers: buildProviderSelections(workspace),
          providerCredentials: buildProviderCredentialDrafts(workspace),
          driveConfig: workspace.drive,
          loading: false,
          status: `Workspace synced at ${new Date(workspace.generatedAt).toLocaleTimeString()}.`
        });
      } catch (error) {
        patchState(store, {
          loading: false,
          status: `Workspace sync failed: ${readErrorMessage(error)}`
        });
      }
    },

    async sendAgentMessage() {
      const message = store.chatDraft().trim();
      const agentKey = store.activeAgentKey();
      if (!message || !agentKey) {
        return;
      }

      patchState(store, { sendingChat: true, status: 'Agent is thinking...' });

      try {
        const response = await firstValueFrom(
          http.post<AgentChatResponse>(`${apiBase}/api/agents/${agentKey}/chat`, { message })
        );

        patchState(store, { chatDraft: '' });
        await this.refreshAll();
        patchState(store, {
          sendingChat: false,
          status: response.blocked ? response.message : 'Agent response received.'
        });
      } catch (error) {
        patchState(store, {
          sendingChat: false,
          status: `Agent request failed: ${readErrorMessage(error)}`
        });
      }
    },

    async approveMemory(memory: MemoryRecord) {
      const draft = this.memoryDraft(memory);
      await firstValueFrom(
        http.post(`${apiBase}/api/memory/${memory.id}/approve`, {
          revisedTitle: draft.title,
          revisedContent: draft.content
        })
      );
      await this.refreshAll();
    },

    async rejectMemory(memory: MemoryRecord) {
      const draft = this.memoryDraft(memory);
      await firstValueFrom(
        http.post(`${apiBase}/api/memory/${memory.id}/reject`, {
          revisedTitle: draft.title,
          revisedContent: draft.content
        })
      );
      await this.refreshAll();
    },

    async moveVideoStage(videoId: string, stage: string) {
      await firstValueFrom(http.post(`${apiBase}/api/videos/${videoId}/stage`, { stage }));
      await this.refreshAll();
    },

    async createManualSchedule() {
      patchState(store, { creatingManualSchedule: true, status: 'Creating manual schedule...' });

      try {
        await firstValueFrom(http.post(`${apiBase}/api/scheduler/manual`, store.manualSchedule()));
        patchState(store, {
          creatingManualSchedule: false,
          manualSchedule: INITIAL_MANUAL_SCHEDULE,
          status: 'Manual schedule created.'
        });
        await this.refreshAll();
      } catch (error) {
        patchState(store, {
          creatingManualSchedule: false,
          status: `Manual schedule failed: ${readErrorMessage(error)}`
        });
      }
    },

    async saveDriveConfig(config: any) {
      patchState(store, { status: 'Saving global Drive configuration...', loadingDrive: true });
      try {
        const saved = await firstValueFrom(http.put<any>(`${apiBase}/api/drive/config`, config));
        patchState(store, { 
          driveConfig: saved,
          loadingDrive: false,
          status: 'Global Drive configuration saved to backend.' 
        });
        await this.loadDriveFiles();
      } catch (error) {
        patchState(store, { 
          loadingDrive: false, 
          status: `Drive config failed: ${readErrorMessage(error)}` 
        });
      }
    },

    async createDriveFolder(name: string) {
      patchState(store, { loadingDrive: true, status: 'Creating folder in Drive...' });
      try {
        const folder = await firstValueFrom(http.post<any>(`${apiBase}/api/drive/folders`, { name }));
        patchState(store, (state) => ({ 
          driveFiles: [folder, ...state.driveFiles],
          loadingDrive: false, 
          status: `Folder '${name}' created.` 
        }));
      } catch (error) {
        patchState(store, { 
          loadingDrive: false, 
          status: `Folder creation failed: ${readErrorMessage(error)}` 
        });
      }
    },

    async loadDriveFiles() {
      if (!store.isDriveConfigured()) return;
      patchState(store, { loadingDrive: true, status: 'Fetching Drive files...' });
      try {
        // Mock data for explorer
        const files = await firstValueFrom(http.get<any[]>(`${apiBase}/api/drive/files`));
        patchState(store, { driveFiles: files, loadingDrive: false, status: 'Drive explorer synced.' });
      } catch (error) {
        patchState(store, { 
          loadingDrive: false, 
          status: `Drive sync failed: ${readErrorMessage(error)}` 
        });
      }
    },

    async saveAgentSettings(agentKey: string) {
      patchState(store, { savingAgentKey: agentKey, status: 'Saving agent settings...' });

      try {
        await firstValueFrom(http.put(`${apiBase}/api/settings/agents/${agentKey}`, this.settingsDraft(agentKey)));
        patchState(store, {
          savingAgentKey: null,
          status: 'Agent settings saved.'
        });
        await this.refreshAll();
      } catch (error) {
        patchState(store, {
          savingAgentKey: null,
          status: `Settings save failed: ${readErrorMessage(error)}`
        });
      }
    }
  }))
);

function emptyProviderSelections(): ProviderSelections {
  return {
    textProvider: PROVIDER_SELECT_OPTIONS.textProvider[0],
    videoProvider: PROVIDER_SELECT_OPTIONS.videoProvider[0],
    uploadProvider: PROVIDER_SELECT_OPTIONS.uploadProvider[0],
    storageProvider: PROVIDER_SELECT_OPTIONS.storageProvider[0]
  };
}

function buildMemoryDrafts(workspace: WorkspaceBootstrap): Record<string, { title: string; content: string }> {
  return Object.fromEntries(
    workspace.memory.reviewQueue.map((memory) => [
      memory.id,
      {
        title: memory.title,
        content: memory.content
      }
    ])
  );
}

function buildSettingsDrafts(workspace: WorkspaceBootstrap): Record<string, SaveAgentSettingsRequest> {
  return Object.fromEntries(workspace.settings.agents.map((agent) => [agent.agentKey, toSettingsRequest(agent)]));
}

function buildProviderSelections(workspace: WorkspaceBootstrap): ProviderSelections {
  const settings = workspace.settings.agents;
  const firstProviderFor = (categories: string[], fallback: string) =>
    settings.find((agent) => categories.includes(agent.category) && agent.providerName.trim())?.providerName || fallback;

  const hasDriveSettings = settings.some(
    (agent) =>
      agent.storageFolderId.trim() ||
      agent.storageFolderPath.trim() ||
      agent.storageFolderUrl.trim() ||
      agent.sourceVideoPath.trim()
  );

  return {
    textProvider: firstProviderFor(['Brain', 'Discovery', 'Writing', 'Shorts'], PROVIDER_SELECT_OPTIONS.textProvider[0]),
    videoProvider: firstProviderFor(['Video'], PROVIDER_SELECT_OPTIONS.videoProvider[0]),
    uploadProvider: firstProviderFor(['Publishing'], PROVIDER_SELECT_OPTIONS.uploadProvider[0]),
    storageProvider: hasDriveSettings ? 'Google Drive' : PROVIDER_SELECT_OPTIONS.storageProvider[0]
  };
}

function buildProviderCredentialDrafts(workspace: WorkspaceBootstrap): Record<string, Record<string, string>> {
  const drafts: Record<string, Record<string, string>> = {};

  for (const agent of workspace.settings.agents) {
    const draft = drafts[agent.providerName] ?? {};
    if (agent.providerName.trim()) {
      drafts[agent.providerName] = mergeProviderCredentials(draft, {
        apiKey: agent.apiKey,
        baseUrl: agent.baseUrl,
        clientId: agent.clientId,
        clientSecret: agent.clientSecret,
        refreshToken: agent.refreshToken,
        openRouterApiKey: agent.openRouterApiKey,
        openRouterModel: agent.openRouterModel
      });
    }

    drafts['Google Drive'] = mergeProviderCredentials(drafts['Google Drive'] ?? {}, {
      clientId: agent.clientId,
      clientSecret: agent.clientSecret,
      refreshToken: agent.refreshToken,
      storageFolderId: agent.storageFolderPath || agent.storageFolderId
    });

    drafts['Local Storage'] = mergeProviderCredentials(drafts['Local Storage'] ?? {}, {
      sourceVideoPath: agent.sourceVideoPath
    });
  }

  return drafts;
}

function mergeProviderCredentials(
  existing: Record<string, string>,
  candidate: Record<string, string>
): Record<string, string> {
  const merged = { ...existing };

  for (const [key, value] of Object.entries(candidate)) {
    if (!merged[key] && value?.trim()) {
      merged[key] = value;
    }
  }

  return merged;
}

function toSettingsRequest(agent: AgentSettings): SaveAgentSettingsRequest {
  return applyDriveMetadata({
    providerName: agent.providerName,
    modelName: agent.modelName,
    baseUrl: agent.baseUrl,
    apiKey: agent.apiKey,
    clientId: agent.clientId,
    clientSecret: agent.clientSecret,
    refreshToken: agent.refreshToken,
    sourceVideoPath: agent.sourceVideoPath,
    storageFolderId: agent.storageFolderId,
    storageFolderName: agent.storageFolderName,
    storageFolderPath: agent.storageFolderPath,
    storageFolderUrl: agent.storageFolderUrl,
    useOpenRouter: agent.useOpenRouter,
    openRouterModel: agent.openRouterModel,
    openRouterApiKey: agent.openRouterApiKey,
    notes: agent.notes
  });
}

function emptySettingsDraft(): SaveAgentSettingsRequest {
  return {
    providerName: '',
    modelName: '',
    baseUrl: '',
    apiKey: '',
    clientId: '',
    clientSecret: '',
    refreshToken: '',
    sourceVideoPath: '',
    storageFolderId: '',
    storageFolderName: '',
    storageFolderPath: '',
    storageFolderUrl: '',
    useOpenRouter: false,
    openRouterModel: '',
    openRouterApiKey: '',
    notes: ''
  };
}

function mergeSettingsDraft<K extends keyof SaveAgentSettingsRequest>(
  draft: SaveAgentSettingsRequest,
  key: K,
  value: SaveAgentSettingsRequest[K]
): SaveAgentSettingsRequest {
  const next = {
    ...draft,
    [key]: value
  } as SaveAgentSettingsRequest;

  if (key === 'storageFolderId' || key === 'storageFolderPath' || key === 'storageFolderUrl' || key === 'storageFolderName') {
    return applyDriveMetadata(next);
  }

  return next;
}

function applyDriveMetadata(draft: SaveAgentSettingsRequest): SaveAgentSettingsRequest {
  const metadata = deriveDriveMetadata(draft.storageFolderId || draft.storageFolderPath, draft.storageFolderUrl);
  return {
    ...draft,
    storageFolderName: metadata.storageFolderName || draft.storageFolderName,
    storageFolderPath: metadata.storageFolderPath || draft.storageFolderPath,
    storageFolderUrl: metadata.storageFolderUrl || draft.storageFolderUrl
  };
}

function deriveDriveMetadata(value: string, existingUrl: string): {
  storageFolderName: string;
  storageFolderPath: string;
  storageFolderUrl: string;
} {
  const raw = value.trim();
  if (!raw) {
    return {
      storageFolderName: '',
      storageFolderPath: '',
      storageFolderUrl: existingUrl
    };
  }

  const segments = raw
    .split(/[/\\>\n]+/)
    .map((segment) => segment.trim())
    .filter(Boolean);
  const storageFolderPath = segments.length ? segments.join(' / ') : raw;
  const storageFolderName = segments.length ? segments[segments.length - 1] : raw;

  return {
    storageFolderName,
    storageFolderPath,
    storageFolderUrl: looksLikeUrl(raw) ? raw : existingUrl
  };
}

function looksLikeUrl(value: string): boolean {
  return /^https?:\/\//i.test(value);
}

function buildSelectedRequirements(selections: ProviderSelections): ProviderRequirement[] {
  const providerNames = [selections.textProvider, selections.videoProvider, selections.uploadProvider, selections.storageProvider]
    .filter((providerName, index, values) => providerName && values.indexOf(providerName) === index);

  return providerNames.map((providerName) => PROVIDER_REQUIREMENTS[providerName] ?? fallbackRequirement(providerName));
}

function fallbackRequirement(providerName: string): ProviderRequirement {
  return {
    providerName,
    displayName: providerName,
    providerType: 'Provider',
    notes: 'No structured credential template is defined for this provider yet.',
    fields: []
  };
}

function buildReadyVideoItems(workspace: WorkspaceBootstrap | null): ReadyVideoListItem[] {
  if (!workspace) {
    return [];
  }

  return workspace.dashboard.readyVideos.flatMap((video) => {
    const platforms = video.platforms.length ? video.platforms : ['Unassigned'];
    return platforms.map((platform) => ({
      id: `${video.id}:${platform}`,
      topic: video.topic,
      platform,
      format: video.format
    }));
  });
}

function getActiveAgentKey(section: TopSectionId, sideTab: SideTabId): string | null {
  if (section === 'agents') {
    return MENU_BY_SECTION.agents.find((item) => item.id === sideTab)?.agentKey ?? null;
  }

  if (section === 'settings') {
    return SETTINGS_NAV_ITEMS.find((item) => item.id === sideTab)?.agentKey ?? null;
  }

  return null;
}

function fallbackProviderOptions(category: string): string[] {
  switch (category) {
    case 'Brain':
    case 'Discovery':
    case 'Writing':
    case 'Shorts':
      return PROVIDER_SELECT_OPTIONS.textProvider;
    case 'Video':
      return PROVIDER_SELECT_OPTIONS.videoProvider;
    case 'Publishing':
      return PROVIDER_SELECT_OPTIONS.uploadProvider;
    default:
      return [];
  }
}

function getBadgeValue(token: string, agentKey: string | undefined, workspace: WorkspaceBootstrap | null): string | number {
  if (!workspace) {
    return '';
  }

  switch (token) {
    case 'overview':
      return 'Live';
    case 'usageCount':
      return workspace.dashboard.usageSeries.length;
    case 'pendingMemory':
      return workspace.memory.reviewQueue.length;
    case 'readyCount':
      return workspace.dashboard.readyVideos.length;
    case 'publishedCount':
      return workspace.dashboard.recentlyPublished.length;
    case 'future':
      return 'Soon';
    case 'agentConnection':
      return workspace.agents.agents.find((agent) => agent.key === agentKey)?.isConnected ? 'On' : 'Setup';
    case 'settingsStatus':
      return workspace.settings.agents.find((agent) => agent.agentKey === agentKey)?.isConnected ? 'Live' : 'API';
    case 'globalMemoryCount':
      return workspace.memory.globalMemories.length;
    case 'localMemoryCount':
      return workspace.memory.localMemories.length;
    case 'manualScheduleCount':
      return workspace.scheduler.manualSchedules.length;
    case 'dailyScheduleCount':
      return workspace.scheduler.dailyPostingJobs.length;
    case 'retryScheduleCount':
      return workspace.scheduler.retryJobs.length;
    case 'queueScheduleCount':
      return workspace.scheduler.queueJobs.length;
    case 'driveFiles':
      return 'Browse';
    case 'driveStatus':
      return 'Config';
    default:
      return '';
  }
}

function readErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === 'object' && error !== null && 'message' in error && typeof error.message === 'string') {
    return error.message;
  }

  return 'Unknown error';
}
