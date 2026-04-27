import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
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
  private http = inject(HttpClient);
  private baseUrl = '/api/agents';
  private memoryUrl = '/api/memory/local';

  startRun(agentKey: string): Observable<{ runId: string }> {
    return this.http.post<{ runId: string }>(`${this.baseUrl}/${agentKey}/run`, {});
  }

  stopRun(agentKey: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${agentKey}/stop`, {});
  }

  sendChat(agentKey: string, message: string): Observable<AgentChatResponse> {
    return this.http.post<AgentChatResponse>(`${this.baseUrl}/${agentKey}/chat`, { message });
  }

  clearChat(agentKey: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${agentKey}/chat/cleanup`);
  }

  streamChat(agentKey: string, message: string): Observable<AgentStreamChunk> {
    // In a real app, you would use EventSource or a custom observable wrapper 
    // around fetch to read the stream. Returning standard observable for now
    // as placeholder.
    return this.http.post<AgentStreamChunk>(`${this.baseUrl}/${agentKey}/chat/stream`, { message });
  }

  getRunHistory(agentKey: string, limit: number = 20): Observable<AgentRun[]> {
    return this.http.get<AgentRun[]>(`${this.baseUrl}/${agentKey}/runs?limit=${limit}`);
  }

  getActiveJob(agentKey: string): Observable<VideoPipelineJob | null> {
    return this.http.get<VideoPipelineJob | null>(`${this.baseUrl}/${agentKey}/active-job`);
  }

  getLocalMemory(agentKey: string): Observable<any> {
    return this.http.get<any>(`${this.memoryUrl}/${agentKey}`);
  }

  updateLocalMemory(agentKey: string, config: any): Observable<void> {
    return this.http.put<void>(`${this.memoryUrl}/${agentKey}`, config);
  }

  getErrorLog(agentKey: string, limit: number = 5): Observable<ErrorLog[]> {
    return this.http.get<ErrorLog[]>(`${this.baseUrl}/${agentKey}/errors?limit=${limit}`);
  }
}
