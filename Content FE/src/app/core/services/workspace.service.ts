import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../constants/api-endpoints';
import { WorkspaceBootstrap } from '../models/content-factory.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly api = inject(ApiService);

  /** Fetches the full studio workspace snapshot used to hydrate every feature store. */
  getBootstrap(): Observable<WorkspaceBootstrap> {
    return this.api.getAction<WorkspaceBootstrap>(ENDPOINTS.WORKSPACE, 'bootstrap');
  }
}
