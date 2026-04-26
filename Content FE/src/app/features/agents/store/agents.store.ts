import { computed, inject, effect } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { AgentsService } from '../services/agents.service';
import { SignalrService } from '../../../core/services/signalr.service';
import { AgentSummary, ChatMessage, WorkspaceBootstrap } from '../../../core/models/content-factory.models';

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
  withMethods((store, 
    agentsSvc = inject(AgentsService),
    signalrSvc = inject(SignalrService)) => {
    
    effect(() => {
      const started = signalrSvc.agentRunStarted();
      const completed = signalrSvc.agentRunCompleted();
      const activeKey = store.activeAgentKey();

      if (started && started.agentKey === activeKey) {
        patchState(store, { isThinking: true, status: 'Agent is executing background task...' });
      }

      if (completed && completed.agentKey === activeKey) {
        patchState(store, { isThinking: false, status: 'Task completed.' });
      }
    });

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
    }
  };
})
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
