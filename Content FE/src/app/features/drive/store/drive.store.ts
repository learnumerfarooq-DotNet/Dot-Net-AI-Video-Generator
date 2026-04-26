import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { ConnectionTestResult, DriveService } from '../services/drive.service';
import { DriveSettings, WorkspaceBootstrap } from '../../../core/models/content-factory.models';

type DriveState = {
  driveConfig: DriveSettings | null;
  driveFiles: any[];
  driveConnection: ConnectionTestResult | null;
  testingDriveConnection: boolean;
  loadingDrive: boolean;
  status: string;
};

const initialState: DriveState = {
  driveConfig: null,
  driveFiles: [],
  driveConnection: null,
  testingDriveConnection: false,
  loadingDrive: false,
  status: 'Ready'
};

export const DriveStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isDriveConfigured: computed(() => !!(store.driveConfig()?.clientId && store.driveConfig()?.refreshToken)),
    isDriveConnected:  computed(() => store.driveConnection()?.success === true),
    driveFileCount:    computed(() => store.driveFiles().length)
  })),
  withMethods((store, driveSvc = inject(DriveService)) => ({
    hydrate(workspace: WorkspaceBootstrap) {
      patchState(store, { driveConfig: workspace.drive });
    },

    async saveDriveConfig(config: Partial<DriveSettings>) {
      patchState(store, { status: 'Saving Drive configuration...', loadingDrive: true });
      try {
        const saved = await firstValueFrom(driveSvc.saveConfig(config));
        patchState(store, { driveConfig: saved, loadingDrive: false, status: 'Drive configuration saved.' });
        await this.testDriveConnection();
        await this.loadDriveFiles();
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Drive config failed: ${readError(error)}` });
      }
    },

    async loadDriveFiles() {
      if (!store.isDriveConfigured()) return;
      patchState(store, { loadingDrive: true, status: 'Fetching Drive files...' });
      try {
        const files = await firstValueFrom(driveSvc.listFiles());
        patchState(store, { driveFiles: files, loadingDrive: false, status: 'Drive explorer synced.' });
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Drive sync failed: ${readError(error)}` });
      }
    },

    async createDriveFolder(name: string) {
      patchState(store, { loadingDrive: true, status: 'Creating folder in Drive...' });
      try {
        const folder = await firstValueFrom(driveSvc.createFolder(name));
        patchState(store, (state) => ({
          driveFiles: [folder, ...state.driveFiles],
          loadingDrive: false,
          status: `Folder '${name}' created.`
        }));
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Folder creation failed: ${readError(error)}` });
      }
    },

    async exchangeOAuthCode(code: string, redirectUri: string) {
      try {
        return await firstValueFrom(driveSvc.exchangeOAuthCode(code, redirectUri));
      } catch (error) {
        patchState(store, { status: `OAuth exchange failed: ${readError(error)}` });
        return null;
      }
    },

    async testDriveConnection() {
      patchState(store, { testingDriveConnection: true, driveConnection: null, status: 'Testing Drive connection...' });
      try {
        const result = await firstValueFrom(driveSvc.testConnection());
        patchState(store, {
          testingDriveConnection: false,
          driveConnection: result,
          status: result.success ? 'Drive connection OK.' : result.message || 'Drive connection failed.'
        });
      } catch (error) {
        patchState(store, {
          testingDriveConnection: false,
          driveConnection: { success: false, message: 'Drive connection test failed.', details: readError(error) },
          status: `Drive test error: ${readError(error)}`
        });
      }
    }
  }))
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
