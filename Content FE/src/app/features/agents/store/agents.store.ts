import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { AgentsService } from '../services/agents.service';
import { AgentSummary, ChatMessage, WorkspaceBootstrap, VideoPipelineJob } from '../../../core/models/content-factory.models';
import { PipelineStore } from '../../../core/store/pipeline.store';
import { SettingsStore } from '../../settings/store/settings.store';
import { AgentWorkspaceService } from '../services/agent-workspace.service';

type AgentsState = {
  agents: AgentSummary[];
  chatMessages: ChatMessage[];
  chatDraft: string;
  sendingChat: boolean;
  streamingContent: string;
  isThinking: boolean;
  activeAgentKey: string | null;
  status: string;
};

const initialState: AgentsState = {
  agents: [],
  chatMessages: [],
  chatDraft: '',
  sendingChat: false,
  streamingContent: '',
  isThinking: false,
  activeAgentKey: null,
  status: 'Ready'
};

export const AgentsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => {
    const pipeline = inject(PipelineStore);
    const settings = inject(SettingsStore);

    return {
      activeAgent: computed<AgentSummary | null>(() => {
        const key = store.activeAgentKey();
        return key ? store.agents().find((a) => a.key === key) ?? null : null;
      }),
      activeAgentMessages: computed<ChatMessage[]>(() => {
        const key = store.activeAgentKey();
        return key ? store.chatMessages().filter((m) => m.agentKey === key) : [];
      }),
      connectedAgentCount: computed(() => store.agents().filter((a) => a.isConnected).length),

      // NEW: Active context for Agent Workspace
      activeJob: computed<VideoPipelineJob | null>(() => {
        const agentKey = store.activeAgentKey();
        if (!agentKey) return null;
        
        return pipeline.jobs().find(j => {
          if (j.status === 'Published' || j.status === 'Failed') {
            return false;
          }

          return (j as any).agentKey === agentKey;
        }) ?? null;
      }),

      targetFolder: computed(() => {
        const agentKey = store.activeAgentKey();
        if (!agentKey) return null;
        
        const agentSettings = settings.agentSettings().find((s: any) => s.agentKey === agentKey);
        return agentSettings ? {
          id: agentSettings.storageFolderId,
          name: agentSettings.storageFolderName,
          path: agentSettings.storageFolderPath,
          url: agentSettings.storageFolderUrl
        } : null;
      })
    };
  }),
  withMethods((store, 
    agentsSvc = inject(AgentsService),
    workspaceSvc = inject(AgentWorkspaceService)) => {
    return {
    hydrate(workspace: WorkspaceBootstrap) {
      patchState(store, {
        agents: workspace.agents.agents,
        chatMessages: workspace.agents.chatMessages
      });
    },

    setActiveAgentKey(key: string | null) {
      patchState(store, { activeAgentKey: key });
    },

    updateChatDraft(value: string) {
      patchState(store, { chatDraft: value });
    },

    async sendAgentMessage(refreshAll: () => Promise<void>) {
      const message = store.chatDraft().trim();
      const agentKey = store.activeAgentKey();
      if (!message || !agentKey) return;

      patchState(store, { 
        sendingChat: true, 
        isThinking: true, 
        streamingContent: '', 
        chatDraft: '',
        status: 'Agent is thinking...' 
      });

      agentsSvc.streamMessage(agentKey, message).subscribe({
        next: (chunk) => {
          if (chunk.type === 'delta') {
            patchState(store, { 
              isThinking: false, 
              streamingContent: store.streamingContent() + chunk.content 
            });
          } else if (chunk.type === 'done') {
            patchState(store, { 
              sendingChat: false, 
              isThinking: false,
              streamingContent: '', 
              status: 'Response received.' 
            });
            refreshAll(); // Refresh to sync full message history and stats
          }
        },
        error: (err) => {
          patchState(store, { 
            sendingChat: false, 
            isThinking: false, 
            status: `Streaming failed: ${readError(err)}` 
          });
        }
      });
    },

    async startAgentRun(agentKey: string) {
      patchState(store, { status: 'Starting agent run...' });
      try {
        await firstValueFrom(workspaceSvc.startRun(agentKey));
        patchState(store, { status: 'Agent run started.' });
      } catch (e) {
        patchState(store, { status: `Failed to start: ${readError(e)}` });
      }
    },

    async stopAgentRun(agentKey: string) {
      try {
        await firstValueFrom(workspaceSvc.stopRun(agentKey));
        patchState(store, { status: 'Agent run stopped.' });
      } catch (e) {
        patchState(store, { status: `Failed to stop: ${readError(e)}` });
      }
    },

    async clearChatHistory(agentKey: string) {
      try {
        await firstValueFrom(workspaceSvc.clearChat(agentKey));
        patchState(store, { 
          chatMessages: store.chatMessages().filter(m => (m as any).agentKey !== agentKey),
          status: 'Chat history cleared.'
        });
      } catch (e) {
        console.error(e);
      }
    },

    async getLocalMemory(agentKey: string) {
      return await firstValueFrom(workspaceSvc.getLocalMemory(agentKey));
    },

    async updateLocalMemory(agentKey: string, config: any) {
      await firstValueFrom(workspaceSvc.updateLocalMemory(agentKey, config));
      patchState(store, { status: 'Local memory updated.' });
    },

    async getRunHistory(agentKey: string) {
      return await firstValueFrom(workspaceSvc.getRunHistory(agentKey));
    },

    async getErrorLog(agentKey: string) {
      return await firstValueFrom(workspaceSvc.getErrorLog(agentKey));
    },

    handleRunStarted(payload: any) {
      if (store.activeAgentKey() === payload.agentKey) {
        patchState(store, { isThinking: true, status: 'Agent started background task...' });
      }
      // Update agent status in the list
      patchState(store, (state) => ({
        agents: state.agents.map(a => a.key === payload.agentKey ? { ...a, status: 'Running' } : a)
      }));
    },

    handleRunCompleted(payload: any) {
      if (store.activeAgentKey() === payload.agentKey) {
        patchState(store, { isThinking: false, status: 'Background task finished.' });
      }
      patchState(store, (state) => ({
        agents: state.agents.map(a => a.key === payload.agentKey ? { ...a, status: 'Idle', lastRunAt: new Date().toISOString() } : a)
      }));
    },

    handleHealthChanged(payload: any) {
      patchState(store, (state) => ({
        agents: state.agents.map(a => a.key === payload.agentKey ? { ...a, status: payload.status ?? payload.newStatus } : a)
      }));
    },

    handleChatResponse(payload: any) {
      if (store.activeAgentKey() === payload.agentKey) {
        patchState(store, (state) => ({
          chatMessages: [...state.chatMessages, payload.message],
          sendingChat: false,
          isThinking: false,
          status: 'Response received.'
        }));
      }
    },

    handleChatStreamChunk(payload: any) {
      if (store.activeAgentKey() === payload.agentKey) {
        if (payload.type === 'delta') {
          patchState(store, { 
            isThinking: false, 
            streamingContent: store.streamingContent() + (payload.chunk ?? payload.content ?? '') 
          });
        } else if (payload.type === 'done') {
          patchState(store, { 
            streamingContent: '',
            isThinking: false
          });
          if (payload.message) {
            patchState(store, (state) => ({
              chatMessages: [...state.chatMessages, payload.message]
            }));
          }
        }
      }
    }
  };
})
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
