import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, inject, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { DriveStore } from './store/drive.store';

@Component({
  selector: 'app-drive-explorer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drive-explorer.html',
  styleUrl: './drive-explorer.css',
  providers: [DriveStore]
})
export class DriveExplorerComponent implements OnInit {
  @Input() isAgentWorkspace = false;
  @Input() initialFolderName: string | null = null;
  
  protected readonly store = inject(ContentFactoryStore);
  protected readonly driveStore = inject(DriveStore);
  private readonly router = inject(Router);

  showNewFolderModal = false;
  showUploadModal = false;
  newFolderName = '';
  selectedLocalFile: File | null = null;
  searchQuery = '';

  ngOnInit() {
    if (this.store.workspace()) {
      this.driveStore.hydrate(this.store.workspace()!);
    }

    if (!this.driveStore.isDriveConfigured()) {
      this.router.navigate(['/drive/drive-config']);
      this.store.setSideTab('drive-config');
      return;
    }
    
    if (this.initialFolderName) {
      this.driveStore.navigateToFolderByName(this.initialFolderName);
    } else {
      this.driveStore.loadDriveFiles();
    }
  }

  getFileIcon(type: string): string {
    if (!type) return 'fa-file';
    const lowerType = type.toLowerCase();
    
    if (lowerType.includes('folder')) return 'fa-folder';
    if (lowerType.includes('video')) return 'fa-file-video';
    if (lowerType.includes('image')) return 'fa-file-image';
    if (lowerType.includes('pdf')) return 'fa-file-pdf';
    if (lowerType.includes('spreadsheet') || lowerType.includes('sheet')) return 'fa-file-excel';
    if (lowerType.includes('presentation') || lowerType.includes('slides')) return 'fa-file-powerpoint';
    if (lowerType.includes('google-doc') || lowerType.includes('document')) return 'fa-file-lines';
    if (lowerType.includes('json') || lowerType.includes('code') || lowerType.includes('script')) return 'fa-file-code';
    
    return 'fa-file';
  }

  async confirmCreateFolder() {
    if (this.newFolderName.trim()) {
      await this.driveStore.createDriveFolder(this.newFolderName.trim());
      this.newFolderName = '';
      this.showNewFolderModal = false;
    }
  }

  async confirmUpload() {
    if (this.selectedLocalFile) {
      await this.driveStore.uploadDriveFile(this.selectedLocalFile);
      this.selectedLocalFile = null;
      this.showUploadModal = false;
    }
  }

  onItemClick(item: any) {
    if (item.isFolder || item.mimeType === 'application/vnd.google-apps.folder') {
      this.driveStore.navigateToFolder(item.id, item.name);
    } else {
      this.driveStore.setSelectedFile(item);
    }
  }

  onBreadcrumbClick(crumb: { id: string | null, name: string }) {
    this.driveStore.navigateToFolder(crumb.id, crumb.name);
  }

  onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedLocalFile = file;
    }
  }

  downloadFile(event: Event, item: any) {
    event.stopPropagation();
    this.driveStore.downloadDriveFile(item);
  }

  deleteFile(event: Event, fileId: string) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this item?')) {
      this.driveStore.deleteDriveFile(fileId);
    }
  }

  startPipeline(event: Event, item: any) {
    event.stopPropagation();
    this.driveStore.startVideoPipeline(item.id, item.name);
  }

  goBack() {
    const breadcrumbs = this.driveStore.breadcrumbs();
    if (breadcrumbs.length > 1) {
      const parent = breadcrumbs[breadcrumbs.length - 2];
      this.driveStore.navigateToFolder(parent.id, parent.name);
    }
  }

  toggleViewMode() {
    const current = this.driveStore.viewMode();
    this.driveStore.setViewMode(current === 'grid' ? 'list' : 'grid');
  }

  setSortBy(sort: 'name' | 'modified' | 'size') {
    this.driveStore.setSortBy(sort);
  }

  get filteredFiles() {
    const q = this.searchQuery.toLowerCase();
    return this.driveStore.driveFiles().filter(f => f.name.toLowerCase().includes(q));
  }
}
