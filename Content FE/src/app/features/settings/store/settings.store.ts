import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import {
  AgentSettings,
  SaveAgentSettingsRequest,
  ProviderSelections,
  ProviderRequirement,
  ProviderRequirementField
} from './settings.models';
import {
  WorkspaceBootstrap,
  PROVIDER_SELECT_OPTIONS
} from '../../../core/models/content-factory.models';
import { SettingsService } from '../services/settings.service';

const PROVIDER_REQUIREMENTS: Record<string, ProviderRequirement> = {
  OpenAI: { providerName: 'OpenAI', displayName: 'OpenAI', providerType: 'Text / reasoning', notes: 'Use an API key and optional custom base URL.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  Claude: { providerName: 'Claude', displayName: 'Claude', providerType: 'Text / writing', notes: 'Claude integrations need an API key.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  Gemini: { providerName: 'Gemini', displayName: 'Gemini', providerType: 'Text / multimodal', notes: 'Gemini can run with a direct API key or custom gateway.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  OpenRouter: { providerName: 'OpenRouter', displayName: 'OpenRouter', providerType: 'Model router', notes: 'OpenRouter adds a routing API key plus a model slug.', fields: [{ id: 'openRouterApiKey', label: 'OpenRouter API Key', inputType: 'password', helpText: 'Paste the OpenRouter API key.' }, { id: 'openRouterModel', label: 'OpenRouter Model', inputType: 'text', helpText: 'Example: openai/gpt-4.1-mini.' }] },
  Runway: { providerName: 'Runway', displayName: 'Runway', providerType: 'Video generation', notes: 'Runway video generation relies on an API key.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  Pika: { providerName: 'Pika', displayName: 'Pika', providerType: 'Video generation', notes: 'Pika connections use an API key.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  Luma: { providerName: 'Luma', displayName: 'Luma', providerType: 'Video generation', notes: 'Luma video flows use an API key.', fields: [{ id: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste the provider API key.' }, { id: 'baseUrl', label: 'Base URL', inputType: 'url', helpText: 'Optional custom gateway or proxy URL.' }] },
  Manual: { providerName: 'Manual', displayName: 'Manual Handoff', providerType: 'No-code fallback', notes: 'No credential required.', fields: [] },
  YouTube: { providerName: 'YouTube', displayName: 'YouTube Upload', providerType: 'Publishing', notes: 'OAuth credentials for YouTube publishing.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }] },
  TikTok: { providerName: 'TikTok', displayName: 'TikTok Upload', providerType: 'Publishing', notes: 'TikTok publishing credentials.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token.' }] },
  Instagram: { providerName: 'Instagram', displayName: 'Instagram Upload', providerType: 'Publishing', notes: 'Instagram OAuth credentials.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token.' }] },
  Facebook: { providerName: 'Facebook', displayName: 'Facebook Upload', providerType: 'Publishing', notes: 'Meta publishing credentials.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token.' }] },
  LinkedIn: { providerName: 'LinkedIn', displayName: 'LinkedIn Upload', providerType: 'Publishing', notes: 'LinkedIn OAuth credentials.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token.' }] },
  DryRun: { providerName: 'DryRun', displayName: 'Dry Run', providerType: 'Publishing fallback', notes: 'Dry run mode skips external publishing.', fields: [] },
  'Google Drive': { providerName: 'Google Drive', displayName: 'Google Drive', providerType: 'Storage', notes: 'Drive credentials and folder metadata.', fields: [{ id: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'OAuth client ID.' }, { id: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'OAuth client secret.' }, { id: 'refreshToken', label: 'Refresh Token', inputType: 'password', helpText: 'Refresh token for long-lived access.' }, { id: 'storageFolderId', label: 'Folder ID / Path', inputType: 'text', helpText: 'Drive folder ID or readable folder tree.' }] },
  'Local Storage': { providerName: 'Local Storage', displayName: 'Local Storage', providerType: 'Storage', notes: 'Local storage uses filesystem paths only.', fields: [{ id: 'sourceVideoPath', label: 'Source Video Path', inputType: 'text', helpText: 'Reusable local folder path.' }] }
};

type SettingsState = {
  agentSettings: AgentSettings[];
  providerOptions: { category: string; providers: string[] }[];
  settingsDrafts: Record<string, SaveAgentSettingsRequest>;
  providers: ProviderSelections;
  providerCredentials: Record<string, Record<string, string>>;
  savingAgentKey: string | null;
  savingProviders: boolean;
  testingAgentKey: string | null;
  testingDrive: boolean;
  testResult: { success: boolean; message: string; details?: string } | null;
  status: string;
};

const initialState: SettingsState = {
  agentSettings: [],
  providerOptions: [],
  settingsDrafts: {},
  providers: emptyProviderSelections(),
  providerCredentials: {},
  savingAgentKey: null,
  savingProviders: false,
  testingAgentKey: null,
  testingDrive: false,
  testResult: null,
  status: 'Ready'
};

export const SettingsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    selectedRequirements: computed<ProviderRequirement[]>(() => buildSelectedRequirements(store.providers())),
    activeAgentSettings: computed<AgentSettings | null>(() => null) // resolved per-component
  })),
  withMethods((store, settingsSvc = inject(SettingsService)) => ({
    hydrate(workspace: WorkspaceBootstrap) {
      patchState(store, {
        agentSettings: workspace.settings.agents,
        providerOptions: workspace.settings.providerOptions,
        settingsDrafts: buildSettingsDrafts(workspace.settings.agents),
        providers: buildProviderSelections(workspace.settings.agents),
        providerCredentials: buildProviderCredentialDrafts(workspace.settings.agents)
      });
    },

    getSettingsForAgent(agentKey: string): AgentSettings | null {
      return store.agentSettings().find((a) => a.agentKey === agentKey) ?? null;
    },

    settingsDraft(agentKey: string): SaveAgentSettingsRequest {
      const existing = store.settingsDrafts()[agentKey];
      if (existing) return existing;
      const agent = store.agentSettings().find((a) => a.agentKey === agentKey);
      return agent ? toSettingsRequest(agent) : emptySettingsDraft();
    },

    updateSettingsDraft<K extends keyof SaveAgentSettingsRequest>(agentKey: string, key: K, value: SaveAgentSettingsRequest[K]) {
      const current = this.settingsDraft(agentKey);
      const next = mergeSettingsDraft(current, key, value);
      patchState(store, {
        settingsDrafts: { ...store.settingsDrafts(), [agentKey]: next }
      });
    },

    updateProviderField<K extends keyof ProviderSelections>(key: K, value: ProviderSelections[K]) {
      patchState(store, { providers: { ...store.providers(), [key]: value } });
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

    getProviderOptionsForAgent(agentKey: string): string[] {
      const agent = store.agentSettings().find((a) => a.agentKey === agentKey);
      const category = agent?.category ?? '';
      const fromWorkspace = store.providerOptions().find((o) => o.category === category)?.providers ?? [];
      return fromWorkspace.length ? fromWorkspace : fallbackProviderOptions(category);
    },

    async saveAgentSettings(agentKey: string, refreshAll: () => Promise<void>) {
      patchState(store, { savingAgentKey: agentKey, status: 'Saving agent settings...' });
      try {
        await firstValueFrom(settingsSvc.saveAgentSettings(agentKey, this.settingsDraft(agentKey)));
        patchState(store, { savingAgentKey: null, status: 'Agent settings saved.' });
        await refreshAll();
      } catch (error) {
        patchState(store, { savingAgentKey: null, status: `Settings save failed: ${readError(error)}` });
      }
    },

    async saveProviders() {
      patchState(store, { savingProviders: true, status: 'Saving provider selections...' });
      try {
        patchState(store, {
          savingProviders: false,
          status: 'Provider selections staged locally. Use agent settings to persist per-agent credentials.'
        });
      } catch (error) {
        patchState(store, { savingProviders: false, status: `Provider save failed: ${readError(error)}` });
      }
    },

    async testAgentConnection(agentKey: string) {
      patchState(store, { testingAgentKey: agentKey, testResult: null, status: 'Testing agent connection...' });
      try {
        const result = await firstValueFrom(settingsSvc.testAgentConnection(agentKey));
        patchState(store, { testingAgentKey: null, testResult: result, status: result.success ? 'Agent connection OK.' : 'Agent connection failed.' });
      } catch (error) {
        patchState(store, { testingAgentKey: null, testResult: { success: false, message: 'Test failed', details: readError(error) }, status: 'Agent test error.' });
      }
    },

    async testDriveConnection() {
      patchState(store, { testingDrive: true, testResult: null, status: 'Testing Drive connection...' });
      try {
        const result = await firstValueFrom(settingsSvc.testDriveConnection());
        patchState(store, { testingDrive: false, testResult: result, status: result.success ? 'Drive connection OK.' : 'Drive connection failed.' });
      } catch (error) {
        patchState(store, { testingDrive: false, testResult: { success: false, message: 'Test failed', details: readError(error) }, status: 'Drive test error.' });
      }
    },

    clearTestResult() {
      patchState(store, { testResult: null });
    }
  }))
);

export { PROVIDER_REQUIREMENTS };

function emptyProviderSelections(): ProviderSelections {
  return {
    textProvider: PROVIDER_SELECT_OPTIONS.textProvider[0],
    videoProvider: PROVIDER_SELECT_OPTIONS.videoProvider[0],
    uploadProvider: PROVIDER_SELECT_OPTIONS.uploadProvider[0],
    storageProvider: PROVIDER_SELECT_OPTIONS.storageProvider[0]
  };
}

function buildSettingsDrafts(agents: AgentSettings[]): Record<string, SaveAgentSettingsRequest> {
  return Object.fromEntries(agents.map((agent) => [agent.agentKey, toSettingsRequest(agent)]));
}

function buildProviderSelections(settings: AgentSettings[]): ProviderSelections {
  const firstProviderFor = (categories: string[], fallback: string) =>
    settings.find((a) => categories.includes(a.category) && a.providerName.trim())?.providerName || fallback;

  const hasDriveSettings = settings.some(
    (a) => a.storageFolderId.trim() || a.storageFolderPath.trim() || a.storageFolderUrl.trim() || a.sourceVideoPath.trim()
  );

  return {
    textProvider: firstProviderFor(['Brain', 'Discovery', 'Writing', 'Shorts'], PROVIDER_SELECT_OPTIONS.textProvider[0]),
    videoProvider: firstProviderFor(['Video'], PROVIDER_SELECT_OPTIONS.videoProvider[0]),
    uploadProvider: firstProviderFor(['Publishing'], PROVIDER_SELECT_OPTIONS.uploadProvider[0]),
    storageProvider: hasDriveSettings ? 'Google Drive' : PROVIDER_SELECT_OPTIONS.storageProvider[0]
  };
}

function buildProviderCredentialDrafts(agents: AgentSettings[]): Record<string, Record<string, string>> {
  const drafts: Record<string, Record<string, string>> = {};
  for (const agent of agents) {
    const draft = drafts[agent.providerName] ?? {};
    if (agent.providerName.trim()) {
      drafts[agent.providerName] = mergeProviderCredentials(draft, {
        apiKey: agent.apiKey, baseUrl: agent.baseUrl, clientId: agent.clientId,
        clientSecret: agent.clientSecret, refreshToken: agent.refreshToken,
        openRouterApiKey: agent.openRouterApiKey, openRouterModel: agent.openRouterModel
      });
    }
    drafts['Google Drive'] = mergeProviderCredentials(drafts['Google Drive'] ?? {}, {
      clientId: agent.clientId, clientSecret: agent.clientSecret,
      refreshToken: agent.refreshToken,
      storageFolderId: agent.storageFolderPath || agent.storageFolderId
    });
    drafts['Local Storage'] = mergeProviderCredentials(drafts['Local Storage'] ?? {}, {
      sourceVideoPath: agent.sourceVideoPath
    });
  }
  return drafts;
}

function mergeProviderCredentials(existing: Record<string, string>, candidate: Record<string, string>): Record<string, string> {
  const merged = { ...existing };
  for (const [key, value] of Object.entries(candidate)) {
    if (!merged[key] && value?.trim()) merged[key] = value;
  }
  return merged;
}

function toSettingsRequest(agent: AgentSettings): SaveAgentSettingsRequest {
  return applyDriveMetadata({
    providerName: agent.providerName, modelName: agent.modelName, baseUrl: agent.baseUrl,
    apiKey: agent.apiKey, clientId: agent.clientId, clientSecret: agent.clientSecret,
    refreshToken: agent.refreshToken, sourceVideoPath: agent.sourceVideoPath,
    storageFolderId: agent.storageFolderId, storageFolderName: agent.storageFolderName,
    storageFolderPath: agent.storageFolderPath, storageFolderUrl: agent.storageFolderUrl,
    useOpenRouter: agent.useOpenRouter, openRouterModel: agent.openRouterModel,
    openRouterApiKey: agent.openRouterApiKey, notes: agent.notes
  });
}

function emptySettingsDraft(): SaveAgentSettingsRequest {
  return {
    providerName: '', modelName: '', baseUrl: '', apiKey: '', clientId: '', clientSecret: '',
    refreshToken: '', sourceVideoPath: '', storageFolderId: '', storageFolderName: '',
    storageFolderPath: '', storageFolderUrl: '', useOpenRouter: false,
    openRouterModel: '', openRouterApiKey: '', notes: ''
  };
}

function mergeSettingsDraft<K extends keyof SaveAgentSettingsRequest>(
  draft: SaveAgentSettingsRequest, key: K, value: SaveAgentSettingsRequest[K]
): SaveAgentSettingsRequest {
  const next = { ...draft, [key]: value } as SaveAgentSettingsRequest;
  if (['storageFolderId', 'storageFolderPath', 'storageFolderUrl', 'storageFolderName'].includes(key as string)) {
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

function deriveDriveMetadata(value: string, existingUrl: string) {
  const raw = value.trim();
  if (!raw) return { storageFolderName: '', storageFolderPath: '', storageFolderUrl: existingUrl };
  const segments = raw.split(/[/\\>\n]+/).map((s) => s.trim()).filter(Boolean);
  const storageFolderPath = segments.length ? segments.join(' / ') : raw;
  const storageFolderName = segments.length ? segments[segments.length - 1] : raw;
  return { storageFolderName, storageFolderPath, storageFolderUrl: /^https?:\/\//i.test(raw) ? raw : existingUrl };
}

function buildSelectedRequirements(selections: ProviderSelections): ProviderRequirement[] {
  const names = [selections.textProvider, selections.videoProvider, selections.uploadProvider, selections.storageProvider]
    .filter((n, i, arr) => n && arr.indexOf(n) === i);
  return names.map((n) => PROVIDER_REQUIREMENTS[n] ?? { providerName: n, displayName: n, providerType: 'Provider', notes: 'No template defined.', fields: [] });
}

function fallbackProviderOptions(category: string): string[] {
  switch (category) {
    case 'Brain': case 'Discovery': case 'Writing': case 'Shorts':
      return PROVIDER_SELECT_OPTIONS.textProvider;
    case 'Video': return PROVIDER_SELECT_OPTIONS.videoProvider;
    case 'Publishing': return PROVIDER_SELECT_OPTIONS.uploadProvider;
    default: return [];
  }
}

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
