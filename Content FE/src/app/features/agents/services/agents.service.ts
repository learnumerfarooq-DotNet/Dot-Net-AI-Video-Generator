import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { AgentChatResponse } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

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

  /** Deletes broken / debug messages from an agent's chat history. */
  cleanupChat(agentKey: string): Observable<CleanupResult> {
    return this.api.delete<CleanupResult>(ENDPOINTS.AGENTS, `${agentKey}/chat/cleanup`);
  }
}
