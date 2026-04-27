import { computed, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { ConnectionTestResult, DriveService } from '../services/drive.service';
import { DriveSettings, WorkspaceBootstrap } from '../../../core/models/content-factory.models';
import { API_BASE, ENDPOINTS } from '../../../core/constants/api-endpoints';

type DriveState = {
  driveConfig: DriveSettings | null;
  driveFiles: any[];
  driveConnection: ConnectionTestResult | null;
  testingDriveConnection: boolean;
  loadingDrive: boolean;
  status: string;
  currentFolderId: string | null;
  breadcrumbs: { id: string | null, name: string }[];
  selectedFile: any | null;
  viewMode: 'grid' | 'list';
  sortBy: 'name' | 'modified' | 'size';
  folderMappings: any | null;
};

const initialState: DriveState = {
  driveConfig: null,
  driveFiles: [],
  driveConnection: null,
  testingDriveConnection: false,
  loadingDrive: false,
  status: 'Ready',
  currentFolderId: null,
  breadcrumbs: [{ id: null, name: 'My Drive' }],
  selectedFile: null,
  viewMode: 'grid',
  sortBy: 'modified',
  folderMappings: null
};

export const DriveStore = signalStore(
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

    async loadDriveFiles(folderId?: string | null) {
      if (!store.isDriveConfigured()) return;
      const targetId = folderId === undefined ? store.currentFolderId() : folderId;
      
      patchState(store, { loadingDrive: true, status: 'Fetching Drive files...' });
      try {
        const files = await firstValueFrom(driveSvc.listFiles(targetId || undefined));
        patchState(store, { 
          driveFiles: files, 
          loadingDrive: false, 
          status: 'Drive explorer synced.',
          currentFolderId: targetId
        });
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Drive sync failed: ${readError(error)}` });
      }
    },

    async navigateToFolder(folderId: string | null, folderName: string) {
      if (folderId === store.currentFolderId()) return;

      // Update breadcrumbs
      if (folderId === null) {
        patchState(store, { breadcrumbs: [{ id: null, name: 'My Drive' }] });
      } else {
        const existing = store.breadcrumbs();
        const index = existing.findIndex(b => b.id === folderId);
        if (index !== -1) {
          patchState(store, { breadcrumbs: existing.slice(0, index + 1) });
        } else {
          patchState(store, { breadcrumbs: [...existing, { id: folderId, name: folderName }] });
        }
      }

      await this.loadDriveFiles(folderId);
    },
    
    async navigateToFolderByName(name: string) {
      if (!store.isDriveConfigured()) return;
      patchState(store, { loadingDrive: true, status: `Finding folder ${name}...` });
      try {
        const files = await firstValueFrom(driveSvc.listFiles(undefined));
        const folder = files.find(f => f.name === name && f.type === 'folder');
        if (folder) {
          await this.navigateToFolder(folder.id, folder.name);
        } else {
           patchState(store, { loadingDrive: false, status: `Folder ${name} not found.` });
        }
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Search failed: ${readError(error)}` });
      }
    },

    async uploadDriveFile(file: File) {
      patchState(store, { loadingDrive: true, status: `Uploading ${file.name}...` });
      try {
        const newFile = await firstValueFrom(driveSvc.uploadFile(file, store.currentFolderId() || undefined));
        patchState(store, (state) => ({
          driveFiles: [newFile, ...state.driveFiles],
          loadingDrive: false,
          status: `File '${file.name}' uploaded successfully.`
        }));
      } catch (error) {
        patchState(store, { loadingDrive: false, status: `Upload failed: ${readError(error)}` });
      }
    },

    async downloadDriveFile(item: any) {
      const fileId = item.id;

      if (!fileId) {
        return;
      }

      const downloadUrl = `${API_BASE}/${ENDPOINTS.DRIVE}/files/${fileId}/download`;
      
      const a = document.createElement('a');
      a.href = downloadUrl;
      a.target = '_blank'; 
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      
      patchState(store, { status: `Started download for ${item.name || 'file'}` });
    },

    async createDriveFolder(name: string) {
      patchState(store, { loadingDrive: true, status: 'Creating folder in Drive...' });
      try {
        const folder = await firstValueFrom(driveSvc.createFolder(name, store.currentFolderId() || undefined));
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
    },

    async deleteDriveFile(fileId: string) {
      patchState(store, { status: 'Deleting file from Drive...' });
      try {
        await firstValueFrom(driveSvc.deleteFile(fileId));
        patchState(store, (state) => ({
          driveFiles: state.driveFiles.filter(f => f.id !== fileId),
          selectedFile: state.selectedFile?.id === fileId ? null : state.selectedFile,
          status: 'File deleted.'
        }));
      } catch (error) {
        patchState(store, { status: `Delete failed: ${readError(error)}` });
      }
    },

    async moveDriveFile(fileId: string, targetFolderId: string) {
      patchState(store, { status: 'Moving file...' });
      try {
        await firstValueFrom(driveSvc.moveFile(fileId, targetFolderId));
        patchState(store, (state) => ({
          driveFiles: state.driveFiles.filter(f => f.id !== fileId),
          status: 'File moved successfully.'
        }));
      } catch (error) {
        patchState(store, { status: `Move failed: ${readError(error)}` });
      }
    },

    async startVideoPipeline(fileId: string, fileName: string) {
      patchState(store, { status: `Starting pipeline for ${fileName}...` });
      try {
        await firstValueFrom(driveSvc.startPipeline(fileId, fileName));
        patchState(store, { status: 'Pipeline initiated successfully.' });
      } catch (error) {
        patchState(store, { status: `Pipeline start failed: ${readError(error)}` });
      }
    },

    async loadFolderMappings() {
      try {
        const mapping = await firstValueFrom(driveSvc.getFolderMapping());
        patchState(store, { folderMappings: mapping });
      } catch (error) {
        console.error('Failed to load folder mappings', error);
      }
    },

    async createMissingFolders() {
      patchState(store, { status: 'Creating missing agent folders...' });
      try {
        await firstValueFrom(driveSvc.createMissingFolders());
        await this.loadFolderMappings();
        patchState(store, { status: 'All missing folders created.' });
      } catch (error) {
        patchState(store, { status: `Folder setup failed: ${readError(error)}` });
      }
    },

    setSelectedFile(file: any | null) {
      patchState(store, { selectedFile: file });
    },

    setViewMode(mode: 'grid' | 'list') {
      patchState(store, { viewMode: mode });
    },

    setSortBy(sort: 'name' | 'modified' | 'size') {
      patchState(store, { sortBy: sort });
    }
  }))
);

function readError(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'object' && error !== null && 'message' in error) return String((error as any).message);
  return 'Unknown error';
}
