import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface AgentDecision {
  id: string;
  agentKey: string;
  type: string;
  outcome: string;
  rawJsonPayload: string;
  validatedPayload: string;
  confidenceScore: number;
  promptVersion: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class DecisionService {
  private http = inject(HttpClient);

  async getDecisions() {
    return await firstValueFrom(this.http.get<AgentDecision[]>('/api/decisions'));
  }

  async getDecisionPayload(id: string) {
    return await firstValueFrom(this.http.get<any>(`/api/decisions/${id}/payload`));
  }

  async approveDecision(id: string) {
    return await firstValueFrom(this.http.post(`/api/decisions/${id}/approve`, {}));
  }

  async rejectDecision(id: string, reason: string) {
    return await firstValueFrom(this.http.post(`/api/decisions/${id}/reject`, { reason }));
  }

  async previewDecision(agentKey: string, type: string, context: any) {
    return await firstValueFrom(this.http.post<AgentDecision>('/api/decisions/preview', { agentKey, type, context }));
  }
}
