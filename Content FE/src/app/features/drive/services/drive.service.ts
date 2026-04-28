import { Injectable, inject } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE, ENDPOINTS } from '../../../core/constants/api-endpoints';
import { DriveSettings } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

export interface DriveFile {
  id:   string;
  name: string;
  type: string;
  size: number;
  date: string;
  isFolder: boolean;
  mimeType?: string;
}

export type DriveFileDto = {
  id: string;
  name: string;
  mimeType: string;
  size: number;
  modifiedTime: string;
  parents: string[];
  webViewLink: string;
  iconLink: string;
  thumbnailLink?: string;
  isFolder: boolean;
  pipelineJobId?: string;
  pipelineStatus?: string;
};

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

  /** Downloads a file from the backend and returns the full response to extract headers. */
  downloadFileWithResponse(fileId: string): Observable<HttpResponse<Blob>> {
    return this.api.getBlobResponse(`${ENDPOINTS.DRIVE}/files/${fileId}/download`);
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

  deleteFile(fileId: string): Observable<void> {
    return this.api.delete<void>(ENDPOINTS.DRIVE, `file/${fileId}`);
  }

  moveFile(fileId: string, targetFolderId: string): Observable<void> {
    return this.api.postAction<void>(ENDPOINTS.DRIVE, `file/${fileId}/move`, { targetFolderId });
  }

  getFolderMapping(): Observable<any> {
    return this.api.getAction<any>(ENDPOINTS.DRIVE, 'mapping');
  }

  createMissingFolders(): Observable<void> {
    return this.api.postAction<void>(ENDPOINTS.DRIVE, 'mapping/create-missing', {});
  }

  getStorageInfo(): Observable<any> {
    return this.api.getAction<any>(ENDPOINTS.DRIVE, 'storage-info');
  }

  startPipeline(fileId: string, fileName: string): Observable<any> {
    return this.api.postAction<any>(ENDPOINTS.DRIVE, 'pipeline/start', { fileId, fileName });
  }

  getQuota(): Observable<{ used: number, limit: number, error: string | null }> {
    return this.api.getAction<any>(ENDPOINTS.DRIVE, 'quota');
  }

  getDownloadUrl(fileId: string): string {
    return `${API_BASE}/${ENDPOINTS.DRIVE}/files/${fileId}/download`;
  }
}
