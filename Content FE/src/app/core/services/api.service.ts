import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from '../constants/api-endpoints';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  private getUrl(endpoint: string, path?: string | number): string {
    return path ? `${API_BASE}/${endpoint}/${path}` : `${API_BASE}/${endpoint}`;
  }

  get<T>(endpoint: string, params?: any): Observable<T> {
    return this.http.get<T>(this.getUrl(endpoint), { params });
  }

  getById<T>(endpoint: string, id: string | number, params?: any): Observable<T> {
    return this.http.get<T>(this.getUrl(endpoint, id), { params });
  }

  post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<T>(this.getUrl(endpoint), body);
  }

  put<T>(endpoint: string, id: string | number, body: unknown): Observable<T> {
    return this.http.put<T>(this.getUrl(endpoint, id), body);
  }

  delete<T>(endpoint: string, id: string | number): Observable<T> {
    return this.http.delete<T>(this.getUrl(endpoint, id));
  }

  // Support for custom actions (e.g. /agents/123/chat)
  postAction<T>(endpoint: string, actionPath: string, body?: unknown): Observable<T> {
    return this.http.post<T>(this.getUrl(endpoint, actionPath), body);
  }

  getAction<T>(endpoint: string, actionPath: string, params?: any): Observable<T> {
    return this.http.get<T>(this.getUrl(endpoint, actionPath), { params });
  }

  getBlob(url: string): Observable<Blob> {
    return this.http.get(this.getUrl(url), { responseType: 'blob' });
  }
}
