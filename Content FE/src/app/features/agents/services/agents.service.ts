import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { AgentChatResponse, AgentStreamChunk } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';
import { API_BASE } from '../../../core/constants/api-endpoints';

export interface SendMessageRequest { message: string; }
export interface CleanupResult    { deleted: number; agentKey: string; }

@Injectable({ providedIn: 'root' })
export class AgentsService {
  private readonly api = inject(ApiService);

  /** Sends a user message to the given agent and returns the reply. */
  sendMessage(agentKey: string, message: string): Observable<AgentChatResponse> {
    return this.api.postAction<AgentChatResponse>(
      ENDPOINTS.AGENTS,
      `${agentKey}/chat`,
      { message } satisfies SendMessageRequest
    );
  }

  /** Streams a user message to the given agent using SSE. */
  streamMessage(agentKey: string, message: string): Observable<AgentStreamChunk> {
    return new Observable<AgentStreamChunk>(observer => {
      const url = `${API_BASE}/${ENDPOINTS.AGENTS}/${agentKey}/chat/stream?message=${encodeURIComponent(message)}`;
      const eventSource = new EventSource(url);

      eventSource.onmessage = (event) => {
        try {
          const chunk = JSON.parse(event.data) as AgentStreamChunk;
          observer.next(chunk);
          if (chunk.type === 'done') {
            eventSource.close();
            observer.complete();
          }
        } catch (err) {
          observer.error('Failed to parse stream chunk');
        }
      };

      eventSource.onerror = (error) => {
        observer.error('Stream connection failed');
        eventSource.close();
      };

      return () => eventSource.close();
    });
  }

  /** Deletes broken / debug messages from an agent's chat history. */
  cleanupChat(agentKey: string): Observable<CleanupResult> {
    return this.api.delete<CleanupResult>(ENDPOINTS.AGENTS, `${agentKey}/chat/cleanup`);
  }
}
