import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { SchedulerService } from '../services/scheduler.service';
import { ScheduleJob, ManualScheduleDraft, WorkspaceBootstrap, INITIAL_MANUAL_SCHEDULE } from '../../../core/models/content-factory.models';

type SchedulerState = {
  manualSchedules: ScheduleJob[];
  dailyPostingJobs: ScheduleJob[];
  retryJobs: ScheduleJob[];
  queueJobs: ScheduleJob[];
  manualSchedule: ManualScheduleDraft;
  creatingManualSchedule: boolean;
  deadLetterQueue: any[];
  queueStats: any | null;
  status: string;
};

const initialState: SchedulerState = {
  manualSchedules: [],
  dailyPostingJobs: [],
  retryJobs: [],
  queueJobs: [],
  manualSchedule: INITIAL_MANUAL_SCHEDULE,
  creatingManualSchedule: false,
  deadLetterQueue: [],
  queueStats: null,
  status: 'Ready'
};

export const SchedulerStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, schedulerSvc = inject(SchedulerService)) => ({
    hydrate(workspace: WorkspaceBootstrap) {
      patchState(store, {
        manualSchedules:  workspace.scheduler.manualSchedules,
        dailyPostingJobs: workspace.scheduler.dailyPostingJobs,
        retryJobs:        workspace.scheduler.retryJobs,
        queueJobs:        workspace.scheduler.queueJobs
      });
    },

    updateManualScheduleField<K extends keyof ManualScheduleDraft>(key: K, value: ManualScheduleDraft[K]) {
      patchState(store, { manualSchedule: { ...store.manualSchedule(), [key]: value } });
    },

    async createManualSchedule(refreshAll: () => Promise<void>) {
      patchState(store, { creatingManualSchedule: true, status: 'Creating manual schedule...' });
      try {
        await firstValueFrom(schedulerSvc.createManual(store.manualSchedule()));
        patchState(store, {
          creatingManualSchedule: false,
          manualSchedule: INITIAL_MANUAL_SCHEDULE,
          status: 'Manual schedule created.'
        });
        await refreshAll();
      } catch (error) {
        patchState(store, { creatingManualSchedule: false, status: `Manual schedule failed: ${readError(error)}` });
      }
    },

    async updateSchedule(id: string, updates: Partial<ScheduleJob>) {
      try {
        await firstValueFrom(schedulerSvc.updateSchedule(id, updates));
        patchState(store, { status: 'Schedule updated.' });
      } catch (e) {
        patchState(store, { status: `Failed to update: ${readError(e)}` });
      }
    },

    async deleteSchedule(id: string) {
      try {
        await firstValueFrom(schedulerSvc.deleteSchedule(id));
        patchState(store, {
          manualSchedules: store.manualSchedules().filter((s: any) => s.id !== id),
          status: 'Schedule deleted.'
        });
      } catch (e) {
        patchState(store, { status: `Failed to delete: ${readError(e)}` });
      }
    },

    async toggleScheduleEnabled(id: string) {
      try {
        await firstValueFrom(schedulerSvc.toggleSchedule(id));
        patchState(store, { status: 'Schedule toggled.' });
      } catch (e) {
        patchState(store, { status: `Failed to toggle: ${readError(e)}` });
      }
    },

    async runScheduleNow(id: string) {
      try {
        await firstValueFrom(schedulerSvc.runNow(id));
        patchState(store, { status: 'Schedule started.' });
      } catch (e) {
        patchState(store, { status: `Failed to run now: ${readError(e)}` });
      }
    },

    async getRetryQueue() {
      try {
        const queue = await firstValueFrom(schedulerSvc.getRetryQueue());
        patchState(store, { retryJobs: queue });
      } catch (e) {
        patchState(store, { status: `Failed to load retry queue: ${readError(e)}` });
      }
    },

    async retryNow(jobId: string) {
      try {
        await firstValueFrom(schedulerSvc.retryNow(jobId));
        patchState(store, { status: 'Job retrying now.' });
        await this.getRetryQueue();
      } catch (e) {
        patchState(store, { status: `Failed to retry job: ${readError(e)}` });
      }
    },

    async moveToDeadLetter(jobId: string) {
      try {
        await firstValueFrom(schedulerSvc.moveToDeadLetter(jobId));
        patchState(store, { status: 'Job moved to dead letter queue.' });
        await this.getRetryQueue();
        await this.getDeadLetterQueue();
      } catch (e) {
        patchState(store, { status: `Failed to move job to dead letter: ${readError(e)}` });
      }
    },

    async getDeadLetterQueue() {
      try {
        const queue = await firstValueFrom(schedulerSvc.getDeadLetterQueue());
        patchState(store, { deadLetterQueue: queue });
      } catch (e) {
        patchState(store, { status: `Failed to load dead letter queue: ${readError(e)}` });
      }
    },

    async resolveDeadLetter(id: string, resolution: string) {
      try {
        await firstValueFrom(schedulerSvc.resolveDeadLetter(id, resolution));
        patchState(store, { status: 'Dead letter resolved.' });
        await this.getDeadLetterQueue();
      } catch (e) {
        patchState(store, { status: `Failed to resolve dead letter: ${readError(e)}` });
      }
    },

    async getQueueStats() {
      try {
        const stats = await firstValueFrom(schedulerSvc.getQueueStats());
        patchState(store, { queueStats: stats });
      } catch (e) {
        patchState(store, { status: `Failed to load queue stats: ${readError(e)}` });
      }
    }
  }))
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
