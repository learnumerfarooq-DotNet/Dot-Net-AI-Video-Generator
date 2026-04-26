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
}
