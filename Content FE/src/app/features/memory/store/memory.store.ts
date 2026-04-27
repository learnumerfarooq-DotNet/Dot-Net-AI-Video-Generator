import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { MemoryService } from '../services/memory.service';
import { AgentLocalMemory, GlobalMemoryFull, MemoryRecord, MemorySuggestionDto, WorkspaceBootstrap } from '../../../core/models/content-factory.models';

type MemoryState = {
  globalMemories: MemoryRecord[];
  localMemories: MemoryRecord[];
  reviewQueue: MemoryRecord[];
  pendingMemorySuggestions: MemorySuggestionDto[];
  memoryDrafts: Record<string, { title: string; content: string }>;
  globalMemoryFull: GlobalMemoryFull | null;
  agentLocalMemories: Record<string, AgentLocalMemory>;
  status: string;
};

const initialState: MemoryState = {
  globalMemories: [],
  localMemories: [],
  reviewQueue: [],
  pendingMemorySuggestions: [],
  memoryDrafts: {},
  globalMemoryFull: null,
  agentLocalMemories: {},
  status: 'Ready'
};

export const MemoryStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    pendingMemoryCount: computed(() => store.reviewQueue().length),
    globalMemoryCount:  computed(() => store.globalMemories().length),
    localMemoryCount:   computed(() => store.localMemories().length)
  })),
  withMethods((store, memorySvc = inject(MemoryService)) => ({
    hydrate(workspace: WorkspaceBootstrap) {
      patchState(store, {
        globalMemories: workspace.memory.globalMemories,
        localMemories:  workspace.memory.localMemories,
        reviewQueue:    workspace.memory.reviewQueue,
        memoryDrafts:   buildMemoryDrafts(workspace.memory.reviewQueue)
      });
    },

    async loadPendingMemorySuggestions() {
      try {
        const data = await firstValueFrom(memorySvc.getPendingSuggestions());
        patchState(store, { pendingMemorySuggestions: data ?? [] });
      } catch { /* swallow in MVP */ }
    },

    memoryDraft(memory: MemoryRecord): { title: string; content: string } {
      return store.memoryDrafts()[memory.id] ?? { title: memory.title, content: memory.content };
    },

    updateMemoryDraft(memory: MemoryRecord, field: 'title' | 'content', value: string) {
      patchState(store, {
        memoryDrafts: {
          ...store.memoryDrafts(),
          [memory.id]: { ...this.memoryDraft(memory), [field]: value }
        }
      });
    },

    async approveMemory(memory: MemoryRecord, refreshAll: () => Promise<void>) {
      const draft = this.memoryDraft(memory);
      await firstValueFrom(memorySvc.approve(memory.id, { revisedTitle: draft.title, revisedContent: draft.content }));
      await refreshAll();
    },

    async rejectMemory(memory: MemoryRecord, refreshAll: () => Promise<void>) {
      const draft = this.memoryDraft(memory);
      await firstValueFrom(memorySvc.reject(memory.id, { revisedTitle: draft.title, revisedContent: draft.content }));
      await refreshAll();
    },

    async loadGlobalMemory() {
      try {
        patchState(store, { status: 'Loading global memory...' });
        const data = await firstValueFrom(memorySvc.getGlobalMemory());
        patchState(store, { globalMemoryFull: data, status: 'Global memory loaded.' });
      } catch (e) {
        patchState(store, { status: 'Failed to load global memory.' });
      }
    },

    async saveGlobalMemory(memory: GlobalMemoryFull) {
      try {
        patchState(store, { status: 'Saving global memory...' });
        const data = await firstValueFrom(memorySvc.updateGlobalMemory(memory));
        patchState(store, { globalMemoryFull: data, status: 'Global memory saved.' });
      } catch (e) {
        patchState(store, { status: 'Failed to save global memory.' });
      }
    },

    async refreshGlobalMemory() {
      try {
        patchState(store, { status: 'Refreshing global memory...' });
        await firstValueFrom(memorySvc.refreshGlobalMemory());
        await this.loadGlobalMemory();
      } catch (e) {
        patchState(store, { status: 'Failed to refresh global memory.' });
      }
    },

    async loadLocalMemory(agentKey: string) {
      try {
        const data = await firstValueFrom(memorySvc.getLocalMemory(agentKey));
        patchState(store, {
          agentLocalMemories: { ...store.agentLocalMemories(), [agentKey]: data }
        });
      } catch (e) {
        console.error(e);
      }
    },

    async saveLocalMemory(agentKey: string, config: any) {
      try {
        patchState(store, { status: `Saving local memory for ${agentKey}...` });
        const data = await firstValueFrom(memorySvc.updateLocalMemory(agentKey, config));
        patchState(store, {
          agentLocalMemories: { ...store.agentLocalMemories(), [agentKey]: data },
          status: 'Local memory saved.'
        });
      } catch (e) {
        patchState(store, { status: 'Failed to save local memory.' });
      }
    },

    async resetLocalMemory(agentKey: string) {
      try {
        await firstValueFrom(memorySvc.resetLocalMemory(agentKey));
        await this.loadLocalMemory(agentKey);
        patchState(store, { status: 'Local memory reset to defaults.' });
      } catch (e) {
        patchState(store, { status: 'Failed to reset local memory.' });
      }
    }
  }))
);

function buildMemoryDrafts(reviewQueue: MemoryRecord[]): Record<string, { title: string; content: string }> {
  return Object.fromEntries(reviewQueue.map((m) => [m.id, { title: m.title, content: m.content }]));
}
