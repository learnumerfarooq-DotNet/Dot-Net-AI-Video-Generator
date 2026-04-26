import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { AgentSettings, SaveAgentSettingsRequest } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly api = inject(ApiService);

  /** Persists provider, model, credential, and storage settings for a single agent. */
  saveAgentSettings(agentKey: string, request: SaveAgentSettingsRequest): Observable<AgentSettings> {
    return this.api.put<AgentSettings>(ENDPOINTS.SETTINGS, `agents/${agentKey}`, request);
  }

  testAgentConnection(agentKey: string): Observable<any> {
    return this.api.postAction<any>(ENDPOINTS.AGENTS, `${agentKey}/connection/test`, {});
  }

  testDriveConnection(): Observable<any> {
    return this.api.postAction<any>(ENDPOINTS.DRIVE, 'connection/test', {});
  }
}
