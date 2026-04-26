import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { ScheduleJob, ManualScheduleDraft } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class SchedulerService {
  private readonly api = inject(ApiService);

  /** Creates a one-off manual schedule entry. */
  createManual(draft: ManualScheduleDraft): Observable<ScheduleJob> {
    return this.api.postAction<ScheduleJob>(ENDPOINTS.SCHEDULER, 'manual', draft);
  }
}
