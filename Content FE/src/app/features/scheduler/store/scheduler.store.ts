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
  status: string;
};

const initialState: SchedulerState = {
  manualSchedules: [],
  dailyPostingJobs: [],
  retryJobs: [],
  queueJobs: [],
  manualSchedule: INITIAL_MANUAL_SCHEDULE,
  creatingManualSchedule: false,
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
    }
  }))
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
