import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { VideoItem } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

export interface UpdateStageRequest { stage: string; }

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = inject(ApiService);

  /** Moves a video to a new stage in the production pipeline. */
  updateVideoStage(videoId: string, stage: string): Observable<VideoItem> {
    return this.api.postAction<VideoItem>(
      ENDPOINTS.VIDEOS,
      `${videoId}/stage`,
      { stage } satisfies UpdateStageRequest
    );
  }

  getSummary(): Observable<any> {
    return this.api.get<any>(ENDPOINTS.DASHBOARD_SUMMARY);
  }

  getVideosByStage(stage: string): Observable<VideoItem[]> {
    return this.api.get<VideoItem[]>(`${ENDPOINTS.DASHBOARD_VIDEOS}/${stage}`);
  }

  getAgentRuns(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.api.get<any>(`${ENDPOINTS.DASHBOARD_RUNS}?page=${page}&pageSize=${pageSize}`);
  }
}
