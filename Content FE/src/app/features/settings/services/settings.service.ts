import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { AgentSettings, SaveAgentSettingsRequest } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly api = inject(ApiService);

  loadAllAgentSettings(): Observable<AgentSettings[]> {
    return this.api.getAction<AgentSettings[]>(ENDPOINTS.SETTINGS, 'agents');
  }

  loadAgentSettings(agentKey: string): Observable<AgentSettings> {
    return this.api.getAction<AgentSettings>(ENDPOINTS.SETTINGS, `agents/${agentKey}`);
  }

  saveAgentSettings(agentKey: string, request: SaveAgentSettingsRequest): Observable<AgentSettings> {
    return this.api.putAction<AgentSettings>(ENDPOINTS.SETTINGS, `agents/${agentKey}`, request);
  }

  testAgentConnection(agentKey: string): Observable<{ success: boolean; message: string; details?: string }> {
    return this.api.postAction<{ success: boolean; message: string; details?: string }>(ENDPOINTS.SETTINGS, `agents/${agentKey}/test`, {});
  }

  testDriveConnection(): Observable<{ success: boolean; message: string; details?: string }> {
    return this.api.postAction<{ success: boolean; message: string; details?: string }>(ENDPOINTS.SETTINGS, `drive/test`, {});
  }

  resetAgentSettings(agentKey: string): Observable<void> {
    return this.api.postAction<void>(ENDPOINTS.SETTINGS, `agents/${agentKey}/reset`, {});
  }

  loadGlobalSettings(): Observable<any> {
    return this.api.getAction<any>(ENDPOINTS.SETTINGS, 'global');
  }

  saveGlobalSettings(settings: any): Observable<any> {
    return this.api.put<any>(ENDPOINTS.SETTINGS, 'global', settings);
  }
  
  getYouTubeAuthUrl(agentKey: string, redirectUri: string): Observable<string> {
    return this.api.getAction<string>(ENDPOINTS.SETTINGS, `youtube/auth-url`, { agentKey, redirectUri });
  }
}
