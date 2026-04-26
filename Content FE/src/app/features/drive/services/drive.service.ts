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

export interface ConnectionTestResult {
  success: boolean;
  message: string;
  details?: string;
}

@Injectable({ providedIn: 'root' })
export class DriveService {
  private readonly api = inject(ApiService);

  /** Exchanges a Google OAuth authorization code for tokens. */
  exchangeOAuthCode(code: string, redirectUri: string): Observable<OAuthExchangeResult> {
    return this.api.postAction<OAuthExchangeResult>(ENDPOINTS.DRIVE, 'oauth/exchange', { code, redirectUri });
  }

  /** Lists files and folders inside the specified Drive folder (or root if none provided). */
  listFiles(folderId?: string): Observable<DriveFile[]> {
    const params = folderId ? { folderId } : {};
    return this.api.getAction<DriveFile[]>(ENDPOINTS.DRIVE, 'files', params);
  }

  /** Uploads a file to the specified Drive folder. */
  uploadFile(file: File, folderId?: string): Observable<DriveFile> {
    const formData = new FormData();
    formData.append('file', file);
    const url = folderId ? `${ENDPOINTS.DRIVE}/files/upload?folderId=${folderId}` : `${ENDPOINTS.DRIVE}/files/upload`;
    return this.api.post<DriveFile>(url, formData);
  }

  /** Downloads a file from the backend (which proxies from Google Drive). */
  downloadFile(fileId: string): Observable<Blob> {
    return this.api.getBlob(`${ENDPOINTS.DRIVE}/files/${fileId}/download`);
  }

  /** Creates a sub-folder inside the specified Drive folder (or root if none provided). */
  createFolder(name: string, folderId?: string): Observable<DriveFile> {
    const url = folderId ? `${ENDPOINTS.DRIVE}/folders?folderId=${folderId}` : `${ENDPOINTS.DRIVE}/folders`;
    return this.api.post<DriveFile>(url, { name });
  }

  /** Persists the global Drive credentials and root folder ID. */
  saveConfig(config: Partial<DriveSettings>): Observable<DriveSettings> {
    return this.api.put<DriveSettings>(ENDPOINTS.DRIVE, 'config', config);
  }

  /** Verifies the saved Drive credentials by making a live backend check. */
  testConnection(): Observable<ConnectionTestResult> {
    return this.api.postAction<ConnectionTestResult>(ENDPOINTS.DRIVE, 'connection/test', {});
  }
}
