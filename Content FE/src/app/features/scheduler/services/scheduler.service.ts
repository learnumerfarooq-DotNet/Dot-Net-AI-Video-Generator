import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ENDPOINTS } from '../../../core/constants/api-endpoints';
import { ScheduleJob, ManualScheduleDraft } from '../../../core/models/content-factory.models';
import { ApiService } from '../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class SchedulerService {
  private readonly api = inject(ApiService);

  getManualSchedules(): Observable<ScheduleJob[]> {
    return this.api.getAction<ScheduleJob[]>('/api/scheduler', 'manual');
  }

  createManual(draft: ManualScheduleDraft): Observable<ScheduleJob> {
    return this.api.postAction<ScheduleJob>('/api/scheduler', 'manual', draft);
  }

  updateSchedule(id: string, updates: Partial<ScheduleJob>): Observable<ScheduleJob> {
    return this.api.putAction<ScheduleJob>('/api/scheduler', id, updates);
  }

  deleteSchedule(id: string): Observable<void> {
    return this.api.delete<void>('/api/scheduler', id);
  }

  toggleSchedule(id: string): Observable<void> {
    return this.api.postAction<void>('/api/scheduler', `${id}/toggle`, {});
  }

  runNow(id: string): Observable<void> {
    return this.api.postAction<void>('/api/scheduler', `${id}/run-now`, {});
  }

  getDailySchedule(): Observable<any[]> {
    return this.api.getAction<any[]>('/api/scheduler', 'daily');
  }

  getRetryQueue(): Observable<any[]> {
    return this.api.getAction<any[]>('/api/scheduler', 'retry');
  }

  retryNow(jobId: string): Observable<void> {
    return this.api.postAction<void>('/api/scheduler', `retry/${jobId}/now`, {});
  }

  moveToDeadLetter(jobId: string): Observable<void> {
    return this.api.postAction<void>('/api/scheduler', `retry/${jobId}/dead-letter`, {});
  }

  getQueueStats(): Observable<any> {
    return this.api.getAction<any>('/api/scheduler', 'queue');
  }

  getDeadLetterQueue(): Observable<any[]> {
    return this.api.getAction<any[]>('/api/scheduler', 'dead-letter');
  }

  resolveDeadLetter(id: string, resolution: string): Observable<void> {
    return this.api.postAction<void>('/api/scheduler', `dead-letter/${id}/resolve`, { resolution });
  }
}
