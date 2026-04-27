import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { VideoPipelineJob, PipelineStageType } from '../models/content-factory.models';

type PipelineState = {
  jobs: VideoPipelineJob[];
  activeJobIds: string[];
  isLoading: boolean;
};

const initialState: PipelineState = {
  jobs: [],
  activeJobIds: [],
  isLoading: false
};

export const PipelineStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    hydrate(jobs: VideoPipelineJob[]) {
      patchState(store, { 
        jobs,
        activeJobIds: jobs.filter(j => j.status !== 'AnalyticsCollected' && j.status !== 'Failed').map(j => j.id)
      });
    },

    handleJobStarted(job: Partial<VideoPipelineJob> & { jobId?: string; fileName?: string; driveFileId?: string }) {
      const normalizedJob: VideoPipelineJob = {
        id: job.id ?? job.jobId ?? crypto.randomUUID(),
        fileName: job.fileName ?? 'New pipeline job',
        status: job.status ?? 'RawDetected',
        currentStage: job.currentStage ?? 'RawDetection',
        currentProgress: job.currentProgress ?? 0,
        stages: job.stages ?? [],
      };

      patchState(store, (state) => ({
        jobs: [normalizedJob, ...state.jobs.filter((existing) => existing.id !== normalizedJob.id)],
        activeJobIds: [...state.activeJobIds.filter((id) => id !== normalizedJob.id), normalizedJob.id]
      }));
    },

    handleStageCompleted(jobId: string, stage: PipelineStageType, progress: number) {
      patchState(store, (state) => ({
        jobs: state.jobs.map(j => 
          j.id === jobId 
            ? { ...j, currentStage: stage, currentProgress: progress } 
            : j
        )
      }));
    },

    handleProgressUpdated(jobId: string, stage: PipelineStageType, percent: number) {
      patchState(store, (state) => ({
        jobs: state.jobs.map(j => 
          j.id === jobId 
            ? { ...j, currentStage: stage, currentProgress: percent } 
            : j
        )
      }));
    },

    handleJobFailed(jobId: string, error: string, retryCount: number) {
      patchState(store, (state) => ({
        jobs: state.jobs.map(j => 
          j.id === jobId 
            ? { ...j, status: 'Failed', retryCount } 
            : j
        ),
        activeJobIds: state.activeJobIds.filter(id => id !== jobId)
      }));
    },

    handleJobCompleted(jobId: string) {
      patchState(store, (state) => ({
        jobs: state.jobs.map(j => 
          j.id === jobId 
            ? { ...j, status: 'Completed', currentProgress: 100 } 
            : j
        ),
        activeJobIds: state.activeJobIds.filter(id => id !== jobId)
      }));
    }
  }))
);
