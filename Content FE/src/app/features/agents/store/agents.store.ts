import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { AgentsService } from '../services/agents.service';
import { AgentSummary, ChatMessage, AgentChatResponse, WorkspaceBootstrap } from '../../../core/models/content-factory.models';

type AgentsState = {
  agents: AgentSummary[];
  chatMessages: ChatMessage[];
  chatDraft: string;
  sendingChat: boolean;
  activeAgentKey: string | null;
  status: string;
};

const initialState: AgentsState = {
  agents: [],
  chatMessages: [],
  chatDraft: '',
  sendingChat: false,
  activeAgentKey: null,
  status: 'Ready'
};

export const AgentsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    activeAgent: computed<AgentSummary | null>(() => {
      const key = store.activeAgentKey();
      return key ? store.agents().find((a) => a.key === key) ?? null : null;
    }),
    activeAgentMessages: computed<ChatMessage[]>(() => {
      const key = store.activeAgentKey();
      return key ? store.chatMessages().filter((m) => m.agentKey === key) : [];
    }),
    connectedAgentCount: computed(() => store.agents().filter((a) => a.isConnected).length)
  })),
  withMethods((store, agentsSvc = inject(AgentsService)) => ({
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

      patchState(store, { sendingChat: true, status: 'Agent is thinking...' });
      try {
        const response = await firstValueFrom(agentsSvc.sendMessage(agentKey, message));
        patchState(store, { chatDraft: '' });
        await refreshAll();
        patchState(store, {
          sendingChat: false,
          status: response.blocked ? response.message : 'Agent response received.'
        });
      } catch (error) {
        patchState(store, { sendingChat: false, status: `Agent request failed: ${readError(error)}` });
      }
    }
  }))
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
