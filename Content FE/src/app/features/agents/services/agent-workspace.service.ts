import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { 
  AgentChatResponse, 
  AgentRun, 
  AgentStreamChunk, 
  ErrorLog, 
  VideoPipelineJob 
} from '../../../core/models/content-factory.models';

@Injectable({
  providedIn: 'root'
})
export class AgentWorkspaceService {
  private api = inject(ApiService);
  private memoryUrl = 'api/memory/local'; // Assuming memory is also on backend

  startRun(agentKey: string): Observable<{ runId: string }> {
    return this.api.postAction<{ runId: string }>(ENDPOINTS.AGENTS, `${agentKey}/run`, {});
  }

  stopRun(agentKey: string): Observable<void> {
    return this.api.postAction<void>(ENDPOINTS.AGENTS, `${agentKey}/stop`, {});
  }

  sendChat(agentKey: string, message: string): Observable<AgentChatResponse> {
    return this.api.postAction<AgentChatResponse>(ENDPOINTS.AGENTS, `${agentKey}/chat`, { message });
  }

  clearChat(agentKey: string): Observable<void> {
    return this.api.postAction<void>(ENDPOINTS.AGENTS, `${agentKey}/chat/cleanup`, {});
  }

  streamChat(agentKey: string, message: string): Observable<AgentStreamChunk> {
    return this.api.postAction<AgentStreamChunk>(ENDPOINTS.AGENTS, `${agentKey}/chat/stream`, { message });
  }

  getRunHistory(agentKey: string, limit: number = 20): Observable<AgentRun[]> {
    return this.api.getAction<AgentRun[]>(ENDPOINTS.AGENTS, `${agentKey}/runs`, { limit });
  }

  getActiveJob(agentKey: string): Observable<VideoPipelineJob | null> {
    return this.api.getAction<VideoPipelineJob | null>(ENDPOINTS.AGENTS, `${agentKey}/active-job`);
  }

  getLocalMemory(agentKey: string): Observable<any> {
    return this.api.getAction<any>(this.memoryUrl, agentKey);
  }

  updateLocalMemory(agentKey: string, config: any): Observable<void> {
    return this.api.putAction<void>(this.memoryUrl, agentKey, config);
  }

  getErrorLog(agentKey: string, limit: number = 5): Observable<ErrorLog[]> {
    return this.api.getAction<ErrorLog[]>(ENDPOINTS.AGENTS, `${agentKey}/errors`, { limit });
  }
}
