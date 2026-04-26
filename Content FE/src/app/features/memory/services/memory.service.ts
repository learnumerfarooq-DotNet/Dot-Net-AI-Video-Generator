import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { MemoryRecord, MemorySuggestionDto } from '../../../core/models/content-factory.models';
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
}
