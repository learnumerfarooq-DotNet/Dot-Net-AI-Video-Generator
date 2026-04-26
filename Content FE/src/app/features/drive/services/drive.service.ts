import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { DriveSettings } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

export interface DriveFile {
  id:   string;
  name: string;
  type: string;
  size: string;
  date: string;
}

export interface OAuthExchangeResult {
  accessToken:  string;
  refreshToken: string;
  expiresIn:    number;
}

@Injectable({ providedIn: 'root' })
export class DriveService {
  private readonly api = inject(ApiService);

  /** Exchanges a Google OAuth authorization code for tokens. */
  exchangeOAuthCode(code: string, redirectUri: string): Observable<OAuthExchangeResult> {
    return this.api.postAction<OAuthExchangeResult>(ENDPOINTS.DRIVE, 'oauth/exchange', { code, redirectUri });
  }

  /** Lists files and folders inside the configured Drive root folder. */
  listFiles(): Observable<DriveFile[]> {
    return this.api.getAction<DriveFile[]>(ENDPOINTS.DRIVE, 'files');
  }

  /** Creates a sub-folder inside the configured Drive root. */
  createFolder(name: string): Observable<DriveFile> {
    return this.api.postAction<DriveFile>(ENDPOINTS.DRIVE, 'folders', { name });
  }

  /** Persists the global Drive credentials and root folder ID. */
  saveConfig(config: Partial<DriveSettings>): Observable<DriveSettings> {
    return this.api.put<DriveSettings>(ENDPOINTS.DRIVE, 'config', config);
  }
}
