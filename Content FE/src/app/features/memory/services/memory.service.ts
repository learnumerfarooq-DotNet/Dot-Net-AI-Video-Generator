import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { AgentLocalMemory, GlobalMemoryFull, MemoryRecord, MemorySuggestionDto } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

export interface ReviewMemoryRequest {
  revisedTitle?:   string;
  revisedContent?: string;
}

@Injectable({ providedIn: 'root' })
export class MemoryService {
  private readonly api = inject(ApiService);

  /** Approves a pending memory record, optionally applying caller revisions. */
  approve(id: string, request: ReviewMemoryRequest): Observable<MemoryRecord> {
    return this.api.postAction<MemoryRecord>(ENDPOINTS.MEMORY, `${id}/approve`, request);
  }

  /** Rejects a pending memory record, optionally applying caller revisions. */
  reject(id: string, request: ReviewMemoryRequest): Observable<MemoryRecord> {
    return this.api.postAction<MemoryRecord>(ENDPOINTS.MEMORY, `${id}/reject`, request);
  }

  /** Returns all records currently in the pending-suggestion queue. */
  getPendingSuggestions(): Observable<MemorySuggestionDto[]> {
    return this.api.getAction<MemorySuggestionDto[]>(ENDPOINTS.MEMORY, 'suggestions/pending');
  }

  getGlobalMemory(): Observable<GlobalMemoryFull> {
    return this.api.getAction<GlobalMemoryFull>('/api/memory', 'global');
  }

  updateGlobalMemory(memory: GlobalMemoryFull): Observable<GlobalMemoryFull> {
    return this.api.putAction<GlobalMemoryFull>('/api/memory', 'global', memory);
  }

  refreshGlobalMemory(): Observable<void> {
    return this.api.postAction<void>('/api/memory', 'global/refresh', {});
  }

  getLocalMemory(agentKey: string): Observable<AgentLocalMemory> {
    return this.api.getAction<AgentLocalMemory>('/api/memory', `local/${agentKey}`);
  }

  updateLocalMemory(agentKey: string, config: any): Observable<AgentLocalMemory> {
    return this.api.putAction<AgentLocalMemory>('/api/memory', `local/${agentKey}`, config);
  }

  resetLocalMemory(agentKey: string): Observable<void> {
    return this.api.postAction<void>('/api/memory', `local/${agentKey}/reset`, {});
  }

  syncLocalMemory(agentKey: string): Observable<void> {
    return this.api.postAction<void>('/api/memory', `local/${agentKey}/sync`, {});
  }
}
