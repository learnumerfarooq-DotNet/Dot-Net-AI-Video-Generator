import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { ErrorLog } from '../models/content-factory.models';

type ErrorState = {
  failures: ErrorLog[];
  unreadCount: number;
};

const initialState: ErrorState = {
  failures: [],
  unreadCount: 0
};

export const ErrorStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    handleNewError(error: ErrorLog) {
      patchState(store, (state) => ({
        failures: [error, ...state.failures],
        unreadCount: state.unreadCount + 1
      }));
    },

    clearUnread() {
      patchState(store, { unreadCount: 0 });
    }
  }))
);
