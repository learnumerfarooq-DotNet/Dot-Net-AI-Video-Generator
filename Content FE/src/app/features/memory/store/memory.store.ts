import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { MemoryService } from '../services/memory.service';
import { MemoryRecord, MemorySuggestionDto, WorkspaceBootstrap } from '../../../core/models/content-factory.models';

type MemoryState = {
  globalMemories: MemoryRecord[];
  localMemories: MemoryRecord[];
  reviewQueue: MemoryRecord[];
  pendingMemorySuggestions: MemorySuggestionDto[];
  memoryDrafts: Record<string, { title: string; content: string }>;
  status: string;
};

const initialState: MemoryState = {
  globalMemories: [],
  localMemories: [],
  reviewQueue: [],
  pendingMemorySuggestions: [],
  memoryDrafts: {},
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
    }
  }))
);

function buildMemoryDrafts(reviewQueue: MemoryRecord[]): Record<string, { title: string; content: string }> {
  return Object.fromEntries(reviewQueue.map((m) => [m.id, { title: m.title, content: m.content }]));
}
