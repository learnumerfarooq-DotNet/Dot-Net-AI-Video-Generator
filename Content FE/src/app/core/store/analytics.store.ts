import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { VideoAnalytics, ViralPattern } from '../models/content-factory.models';

type AnalyticsState = {
  stats: VideoAnalytics[];
  patterns: ViralPattern[];
  isLoading: boolean;
};

const initialState: AnalyticsState = {
  stats: [],
  patterns: [],
  isLoading: false
};

export const AnalyticsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    hydrate(stats: VideoAnalytics[], patterns: ViralPattern[]) {
      patchState(store, { stats, patterns });
    },

    loadDailyReport() {
      // Simulate API call
      patchState(store, { isLoading: true });
      setTimeout(() => {
        patchState(store, { isLoading: false });
      }, 1000);
    }
  }))
);
